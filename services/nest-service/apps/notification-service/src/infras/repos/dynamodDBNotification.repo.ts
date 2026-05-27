// src/notification/infras/repos/notification.repo.ts
import { Injectable } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import {
  DynamoDBDocumentClient,
  PutCommand,
  QueryCommand,
  ScanCommand,
  UpdateCommand,
} from '@aws-sdk/lib-dynamodb';
import { NotificationDomain } from '@notification-service/core/models/notification';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import {
  BulkUpdateNotificationParams,
  GetAllNotificationParams,
  UpdateNotificationParams,
} from '@notification-service/core/services/notification.service';
import { NotificationMapper } from '@notification-service/infras/mapper';
import { v4 as uuidv4 } from 'uuid';

@Injectable()
export class DynamoDBNotificationRepo implements INotificationRepo {
  private readonly docClient: DynamoDBDocumentClient;
  private readonly tableName = 'taskflow';

  constructor(private readonly configService: ConfigService) {
    const region = this.configService.get<string>('AWS_REGION', 'ap-southeast-1');
    const accessKeyId = this.configService.get<string>('AWS_ACCESS_KEY_ID');
    const secretAccessKey = this.configService.get<string>('AWS_SECRET_ACCESS_KEY');

    const endpoint = this.configService.get<string>('DYNAMODB_ENDPOINT');

    const clientConfig: any = { region };

    if (accessKeyId && secretAccessKey) {
      clientConfig.credentials = { accessKeyId, secretAccessKey };
    }

    if (endpoint) {
      clientConfig.endpoint = endpoint;
    }

    const client = new DynamoDBClient(clientConfig);
    this.docClient = DynamoDBDocumentClient.from(client, {
      marshallOptions: {
        removeUndefinedValues: true,
      },
    });
  }

  private mapToMongoSchemaShape(item: any): any {
    return {
      _id: item.id,
      recipientId: item.recipentId,
      actorId: item.actorId,
      type: item.type,
      referenceId: item.referenceId,
      referenceType: item.referenceType,
      content: item.content,
      isRead: item.isRead,
      createdAt: new Date(item.createdAt),
    };
  }

  async create(notification: NotificationDomain): Promise<NotificationDomain> {
    const notiId = notification.id || uuidv4();
    const createdAtTs = notification.createdAt
      ? new Date(notification.createdAt).getTime()
      : Date.now();
    const updatedAtTs = Date.now();

    const item = {
      id: notiId,
      recipentId: notification.recipientId,
      actorId: notification.actorId || '',
      type: notification.type,
      referenceId: notification.referenceId || '',
      referenceType: notification.referenceType || '',
      content: notification.content || '',
      isRead: notification.isRead ?? false,
      createdAt: createdAtTs,
      updatedAt: updatedAtTs,
    };

    await this.docClient.send(
      new PutCommand({
        TableName: this.tableName,
        Item: item,
      }),
    );

    notification.id = notiId;
    return notification;
  }

  async countAll(params?: GetAllNotificationParams): Promise<number> {
    if (!params?.userId) {
      return 0;
    }

    const queryParams: any = {
      TableName: this.tableName,
      KeyConditionExpression: 'recipentId = :recipientId',
      ExpressionAttributeValues: {
        ':recipientId': params.userId,
      },
    };

    const filterExpressions: string[] = [];
    if (params.projectId) {
      filterExpressions.push('referenceType = :refType AND referenceId = :refId');
      queryParams.ExpressionAttributeValues[':refType'] = 'project';
      queryParams.ExpressionAttributeValues[':refId'] = params.projectId;
    } else if (params.sprintId) {
      filterExpressions.push('referenceType = :refType AND referenceId = :refId');
      queryParams.ExpressionAttributeValues[':refType'] = 'sprint';
      queryParams.ExpressionAttributeValues[':refId'] = params.sprintId;
    }

    if (filterExpressions.length > 0) {
      queryParams.FilterExpression = filterExpressions.join(' AND ');
    }

    const result = await this.docClient.send(new QueryCommand(queryParams));
    return result.Count ?? 0;
  }

  async listAll(params: GetAllNotificationParams): Promise<NotificationDomain[]> {
    if (!params.userId) {
      return [];
    }

    const queryParams: any = {
      TableName: this.tableName,
      KeyConditionExpression: 'recipentId = :recipientId',
      ExpressionAttributeValues: {
        ':recipientId': params.userId,
      },
      ScanIndexForward: false,
    };

    const filterExpressions: string[] = [];
    if (params.projectId) {
      filterExpressions.push('referenceType = :refType AND referenceId = :refId');
      queryParams.ExpressionAttributeValues[':refType'] = 'project';
      queryParams.ExpressionAttributeValues[':refId'] = params.projectId;
    } else if (params.sprintId) {
      filterExpressions.push('referenceType = :refType AND referenceId = :refId');
      queryParams.ExpressionAttributeValues[':refType'] = 'sprint';
      queryParams.ExpressionAttributeValues[':refId'] = params.sprintId;
    }

    if (filterExpressions.length > 0) {
      queryParams.FilterExpression = filterExpressions.join(' AND ');
    }

    const result = await this.docClient.send(new QueryCommand(queryParams));
    const items = result.Items ?? [];

    const page = params.page ?? 1;
    const limit = params.limit ?? 10;
    const skip = (page - 1) * limit;

    const paginatedItems = items.slice(skip, skip + limit);
    const mongoShapeItems = paginatedItems.map((item) => this.mapToMongoSchemaShape(item));

    return NotificationMapper.toDomainList(mongoShapeItems);
  }

  async update(params: UpdateNotificationParams): Promise<NotificationDomain> {
    const { notiId, isRead } = params;

    // Tìm kiếm phần tử bằng ID để lấy khoá phân vùng (recipentId) và sort key (createdAt)
    const scanResult = await this.docClient.send(
      new ScanCommand({
        TableName: this.tableName,
        FilterExpression: 'id = :id',
        ExpressionAttributeValues: {
          ':id': notiId,
        },
      }),
    );

    const item = scanResult.Items?.[0];
    if (!item) {
      throw new Error(`Notification with ID ${notiId} not found.`);
    }

    const updatedResult = await this.docClient.send(
      new UpdateCommand({
        TableName: this.tableName,
        Key: {
          recipentId: item.recipentId,
          createdAt: item.createdAt,
        },
        UpdateExpression: 'set isRead = :isRead, updatedAt = :updatedAt',
        ExpressionAttributeValues: {
          ':isRead': isRead,
          ':updatedAt': Date.now(),
        },
        ReturnValues: 'ALL_NEW',
      }),
    );

    const updatedItem = updatedResult.Attributes;
    return NotificationMapper.toDomain(this.mapToMongoSchemaShape(updatedItem));
  }

  async bulkUpdate(params: BulkUpdateNotificationParams): Promise<{ success: boolean }> {
    const { userId, isRead } = params;

    // Lấy toàn bộ danh sách thông báo của user
    const queryResult = await this.docClient.send(
      new QueryCommand({
        TableName: this.tableName,
        KeyConditionExpression: 'recipentId = :recipientId',
        ExpressionAttributeValues: {
          ':recipientId': userId,
        },
      }),
    );

    const items = queryResult.Items ?? [];
    if (items.length === 0) {
      throw new Error(`Notification not found with this user`);
    }

    const itemsToUpdate = items.filter((item) => item.isRead !== isRead);
    if (itemsToUpdate.length === 0) {
      return { success: true };
    }

    // Thực hiện cập nhật song song
    await Promise.all(
      itemsToUpdate.map((item) =>
        this.docClient.send(
          new UpdateCommand({
            TableName: this.tableName,
            Key: {
              recipentId: item.recipentId,
              createdAt: item.createdAt,
            },
            UpdateExpression: 'set isRead = :isRead, updatedAt = :updatedAt',
            ExpressionAttributeValues: {
              ':isRead': isRead,
              ':updatedAt': Date.now(),
            },
          }),
        ),
      ),
    );

    return { success: true };
  }
}

