import { Injectable } from '@nestjs/common';
import { NotificationDomain, NotificationType, ReferenceType } from '@notification-service/core/models/notification';
import { INotificationRepo } from '@notification-service/core/interfaces/notification-repo.interface';
import { RpcException } from '@nestjs/microservices';
import { status } from '@grpc/grpc-js';
import { IssueClientService, ProjectClientService, SprintClientService, UserClientService } from '@nest-service/core';
import { ProjectRes } from '@nest-service/core/types/main_service/project';
import { SprintRes } from '@nest-service/core/types/main_service/sprint';
import { IssueRes } from '@nest-service/core/types/base';
export interface CreateNotificationParams {
  recipientId: string;
  actorId?: string;
  type: NotificationType;
  referenceId?: string;
  content: string;
  isRead: boolean;
  createdAt: Date;
}
export interface GetAllNotificationParams {
  userId?: string | undefined;
  projectId?: string | undefined;
  sprintId?: string | undefined;
  page?: number | undefined;
  limit?: number | undefined;
}

@Injectable()
export class NotificationService {
  constructor(
    private readonly notificationRepo: INotificationRepo,
    private readonly projectClientService: ProjectClientService,
    private readonly sprintCLientService: SprintClientService,
    private readonly issueClientService: IssueClientService,
    private readonly userClientService: UserClientService,
  ) {}

  async createNotification(data: CreateNotificationParams): Promise<NotificationDomain> {
    const refType = this.getReferenceTypeByNotification(data.type);
    return await this.notificationRepo.create({
      recipientId: data.recipientId,
      actorId: data.actorId,
      type: data.type,
      referenceId: data.referenceId,
      referenceType: refType,
      content: data?.content || '',
      isRead: false,
      createdAt: new Date(),
    });
  }

  async listNotifications(params: GetAllNotificationParams): Promise<{ data: NotificationDomain[]; totalCount: number }> {
    const [notiDomain, totalCount] = await Promise.all([this.notificationRepo.listAll(params), this.notificationRepo.countAll(params)]);
    const issueIds: string[] = [];
    const projectIds: string[] = [];
    const sprintIds: string[] = [];

    notiDomain.forEach((noti) => {
      if (noti.referenceType == ReferenceType.ISSUE && noti.referenceId) {
        issueIds.push(noti.referenceId);
      }
      if (noti.referenceType == ReferenceType.PROJECT && noti.referenceId) {
        projectIds.push(noti.referenceId);
      }
      if (noti.referenceType == ReferenceType.SPRINT && noti.referenceId) {
        sprintIds.push(noti.referenceId);
      }
    });

    const [projects, sprints, issues] = await Promise.all([
      this.projectClientService.getListProjects(projectIds),
      this.sprintCLientService.getListSprints(sprintIds),
      this.issueClientService.getListIssues(issueIds),
    ]);

    const projectsMap = new Map<string, ProjectRes>();
    (projects || []).forEach((project) => {
      projectsMap.set(project.id, project);
    });

    const sprintsMap = new Map<string, SprintRes>();
    (sprints || []).forEach((sprint) => {
      sprintsMap.set(sprint.id, sprint);
    });

    const issuesMaps = new Map<string, IssueRes>();
    (issues || []).forEach((issue) => {
      issuesMaps.set(issue.id, issue);
    });

    notiDomain.forEach((noti) => {
      if (noti.referenceType === ReferenceType.PROJECT && noti.referenceId) {
        const project = projectsMap.get(noti.referenceId);
        if (project) {
          noti.referenceData = project;
        }
      }
      if (noti.referenceType === ReferenceType.SPRINT && noti.referenceId) {
        const sprint = sprintsMap.get(noti.referenceId);
        if (sprint) {
          noti.referenceData = sprint;
        }
      }
      if (noti.referenceType === ReferenceType.ISSUE && noti.referenceId) {
        const issue = issuesMaps.get(noti.referenceId);
        if (issue) {
          noti.referenceData = issue;
        }
      }
      // ...
    });
    return {
      data: notiDomain,
      totalCount: totalCount,
    };
  }

  private getReferenceTypeByNotification(type: NotificationType): ReferenceType {
    switch (type) {
      case NotificationType.ASSIGNMENT:
      case NotificationType.MENTION:
      case NotificationType.COMMENT:
      case NotificationType.STATUS_UPDATE:
      case NotificationType.DUE_DATE_REMINDER:
        return ReferenceType.ISSUE;

      case NotificationType.SPRINT_STARTED:
        return ReferenceType.SPRINT;

      case NotificationType.PROJECT_INVITATION:
      case NotificationType.PROJECT_ADDED:
        return ReferenceType.PROJECT;

      case NotificationType.REACTION:
        return ReferenceType.COMMENT;

      case NotificationType.PROJECT_TEAM_ADDED:
        return ReferenceType.PROJECT_MEMBER;

      case NotificationType.SYSTEM_ALERT:
        return ReferenceType.SYSTEM;

      default:
        return ReferenceType.SYSTEM;
    }
  }

  ValidateNotificationType(type: string): NotificationType {
    if (!type || !Object.values(NotificationType).includes(type as NotificationType)) {
      throw new RpcException({
        code: status.INVALID_ARGUMENT,
        message: 'Invalid notification type',
      });
    }
    return type as NotificationType;
  }
}
