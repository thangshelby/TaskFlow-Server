/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ([
/* 0 */,
/* 1 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __exportStar = (this && this.__exportStar) || function(m, exports) {
    for (var p in m) if (p !== "default" && !Object.prototype.hasOwnProperty.call(exports, p)) __createBinding(exports, m, p);
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
__exportStar(__webpack_require__(2), exports);
__exportStar(__webpack_require__(5), exports);
__exportStar(__webpack_require__(6), exports);
__exportStar(__webpack_require__(13), exports);
__exportStar(__webpack_require__(22), exports);
__exportStar(__webpack_require__(24), exports);
__exportStar(__webpack_require__(26), exports);
__exportStar(__webpack_require__(29), exports);
__exportStar(__webpack_require__(15), exports);
__exportStar(__webpack_require__(20), exports);
__exportStar(__webpack_require__(21), exports);
__exportStar(__webpack_require__(19), exports);
__exportStar(__webpack_require__(30), exports);


/***/ }),
/* 2 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.CoreModule = void 0;
const config_1 = __webpack_require__(3);
const common_1 = __webpack_require__(4);
const log_service_1 = __webpack_require__(5);
const kafka_service_1 = __webpack_require__(6);
const aws_service_1 = __webpack_require__(9);
const aws_queue_adapter_1 = __webpack_require__(11);
const kafka_queue_adapter_1 = __webpack_require__(12);
const queue_interface_1 = __webpack_require__(13);
const mongoose_1 = __webpack_require__(14);
const user_client_service_1 = __webpack_require__(15);
const microservices_1 = __webpack_require__(16);
const path_1 = __webpack_require__(18);
const project_client_service_1 = __webpack_require__(19);
const sprint_client_service_1 = __webpack_require__(20);
const issue_client_service_1 = __webpack_require__(21);
const mainServiceGrpcUrl = (config) => config.get('MAIN_SERVICE')?.trim() || 'main-service.TaskFlowNameSpace';
let CoreModule = class CoreModule {
};
exports.CoreModule = CoreModule;
exports.CoreModule = CoreModule = __decorate([
    (0, common_1.Module)({
        controllers: [],
        imports: [
            config_1.ConfigModule.forRoot({
                isGlobal: true,
            }),
            microservices_1.ClientsModule.registerAsync([
                {
                    name: 'USER_PACKAGE',
                    imports: [config_1.ConfigModule],
                    inject: [config_1.ConfigService],
                    useFactory: (configService) => ({
                        transport: microservices_1.Transport.GRPC,
                        options: {
                            package: 'project_service',
                            protoPath: (0, path_1.join)(__dirname, 'protos/main_service/user.proto'),
                            loader: {
                                includeDirs: [(0, path_1.join)(__dirname, 'protos')],
                            },
                            url: mainServiceGrpcUrl(configService),
                        },
                    }),
                },
                {
                    name: 'PROJECT_PACKAGE',
                    imports: [config_1.ConfigModule],
                    inject: [config_1.ConfigService],
                    useFactory: (configService) => ({
                        transport: microservices_1.Transport.GRPC,
                        options: {
                            package: 'project_service',
                            protoPath: (0, path_1.join)(__dirname, 'protos/main_service/project.proto'),
                            loader: {
                                includeDirs: [(0, path_1.join)(__dirname, 'protos')],
                            },
                            url: mainServiceGrpcUrl(configService),
                        },
                    }),
                },
                {
                    name: 'SPRINT_PACKAGE',
                    imports: [config_1.ConfigModule],
                    inject: [config_1.ConfigService],
                    useFactory: (configService) => ({
                        transport: microservices_1.Transport.GRPC,
                        options: {
                            package: 'project_service',
                            protoPath: (0, path_1.join)(__dirname, 'protos/main_service/sprint.proto'),
                            loader: {
                                includeDirs: [(0, path_1.join)(__dirname, 'protos')],
                            },
                            url: mainServiceGrpcUrl(configService),
                        },
                    }),
                },
                {
                    name: 'ISSUE_PACKAGE',
                    imports: [config_1.ConfigModule],
                    inject: [config_1.ConfigService],
                    useFactory: (configService) => ({
                        transport: microservices_1.Transport.GRPC,
                        options: {
                            package: 'project_service',
                            protoPath: (0, path_1.join)(__dirname, 'protos/main_service/issue.proto'),
                            loader: {
                                includeDirs: [(0, path_1.join)(__dirname, 'protos')],
                            },
                            url: mainServiceGrpcUrl(configService),
                        },
                    }),
                },
            ]),
            mongoose_1.MongooseModule.forRootAsync({
                imports: [config_1.ConfigModule],
                inject: [config_1.ConfigService],
                useFactory: (configService) => ({
                    uri: configService.get('MONGODB_URI'),
                    dbName: configService.get('MONGODB_DB_NAME'),
                }),
            }),
        ],
        providers: [
            log_service_1.LogService,
            kafka_service_1.KafkaService,
            aws_service_1.AwsService,
            kafka_queue_adapter_1.KafkaQueueAdapter,
            aws_queue_adapter_1.AwsQueueAdapter,
            {
                provide: queue_interface_1.QUEUE_SERVICE_TOKEN,
                useFactory: (configService, kafkaQueueAdapter, awsQueueAdapter) => {
                    const provider = (configService.get('QUEUE_PROVIDER') ?? 'aws').toLowerCase().trim();
                    return provider === 'kafka' ? kafkaQueueAdapter : awsQueueAdapter;
                },
                inject: [config_1.ConfigService, kafka_queue_adapter_1.KafkaQueueAdapter, aws_queue_adapter_1.AwsQueueAdapter],
            },
            user_client_service_1.UserClientService,
            project_client_service_1.ProjectClientService,
            sprint_client_service_1.SprintClientService,
            issue_client_service_1.IssueClientService,
        ],
        exports: [log_service_1.LogService, kafka_service_1.KafkaService, aws_service_1.AwsService, queue_interface_1.QUEUE_SERVICE_TOKEN, user_client_service_1.UserClientService, project_client_service_1.ProjectClientService, sprint_client_service_1.SprintClientService, issue_client_service_1.IssueClientService],
    })
], CoreModule);


/***/ }),
/* 3 */
/***/ ((module) => {

module.exports = require("@nestjs/config");

/***/ }),
/* 4 */
/***/ ((module) => {

module.exports = require("@nestjs/common");

/***/ }),
/* 5 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var LogService_1;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.LogService = void 0;
const common_1 = __webpack_require__(4);
let LogService = LogService_1 = class LogService {
    logger = new common_1.Logger(LogService_1.name);
    log(message) {
        this.logger.log(message);
    }
    error(message, trace) {
        this.logger.error(message, trace);
    }
    warn(message) {
        this.logger.warn(message);
    }
};
exports.LogService = LogService;
exports.LogService = LogService = LogService_1 = __decorate([
    (0, common_1.Injectable)()
], LogService);


/***/ }),
/* 6 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a, _b;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.KafkaService = exports.KafkaActionType = void 0;
const common_1 = __webpack_require__(4);
const kafkajs_1 = __webpack_require__(7);
const uuid_1 = __webpack_require__(8);
const log_service_1 = __webpack_require__(5);
const config_1 = __webpack_require__(3);
var KafkaActionType;
(function (KafkaActionType) {
    KafkaActionType["NOTIFICATIONS_CREATE_NEW_NOTIFICATION"] = "NOTIFICATIONS_CREATE_NEW_NOTIFICATION";
    KafkaActionType["MAILS_SEND_VERIFY_OTP_USER"] = "MAILS_SEND_VERIFY_OTP_USER";
})(KafkaActionType || (exports.KafkaActionType = KafkaActionType = {}));
let KafkaService = class KafkaService {
    logService;
    configService;
    kafka;
    producer;
    consumers = [];
    producerConnected = false;
    constructor(logService, configService) {
        this.logService = logService;
        this.configService = configService;
        const clientId = this.configService.get('KAFKA_CLIENT_ID', 'taskflow-client');
        const brokers = this.configService.get('KAFKA_BROKERS', 'localhost:29092').split(',');
        this.kafka = new kafkajs_1.Kafka({
            clientId,
            brokers,
        });
        this.producer = this.kafka.producer({
            createPartitioner: kafkajs_1.Partitioners.LegacyPartitioner,
        });
    }
    async ensureProducerConnected() {
        if (this.producerConnected) {
            return;
        }
        await this.producer.connect();
        this.producerConnected = true;
        this.logService.log('Kafka Producer connected');
    }
    async onModuleDestroy() {
        for (const consumer of this.consumers) {
            await consumer.disconnect();
        }
        if (this.producerConnected) {
            await this.producer.disconnect();
        }
        this.logService.log('Kafka disconnected');
    }
    async publish(topic, message, config = {}) {
        await this.ensureProducerConnected();
        try {
            const kafkaMessage = {
                value: JSON.stringify(message),
                key: config.partitionKey ?? `part_${(0, uuid_1.v4)()}`,
            };
            await this.producer.send({
                topic,
                messages: [kafkaMessage],
            });
            this.logService.log(`Message published to topic ${topic}: ${JSON.stringify(message)}`);
        }
        catch (error) {
            this.logService.error(`Failed to publish message to ${topic}`, error.stack);
            throw error;
        }
    }
    async emit(topic, message, config) {
        if (!config && !('id' in message)) {
            throw new Error('Message id is required');
        }
        await this.publish(topic, message, config);
    }
    async subscribe(topic, callback) {
        const baseGroupId = this.configService.get('KAFKA_GROUP_ID', 'taskflow-group');
        const consumer = this.kafka.consumer({ groupId: `${baseGroupId}-${topic}` });
        await consumer.connect();
        await consumer.subscribe({ topic, fromBeginning: true });
        await consumer.run({
            eachMessage: async (payload) => {
                try {
                    await callback(payload);
                    this.logService.log(`Processed message from ${topic} [${payload.partition}]: ${payload.message.value?.toString() || ''}`);
                }
                catch (error) {
                    this.logService.error(`Error processing message from ${topic}`, error.stack);
                }
            },
        });
        this.consumers.push(consumer);
        this.logService.log(`Subscribed to topic: ${topic}`);
    }
    on(topic, callback) {
        void this.subscribe(topic, callback);
    }
};
exports.KafkaService = KafkaService;
exports.KafkaService = KafkaService = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof log_service_1.LogService !== "undefined" && log_service_1.LogService) === "function" ? _a : Object, typeof (_b = typeof config_1.ConfigService !== "undefined" && config_1.ConfigService) === "function" ? _b : Object])
], KafkaService);


/***/ }),
/* 7 */
/***/ ((module) => {

module.exports = require("kafkajs");

/***/ }),
/* 8 */
/***/ ((module) => {

module.exports = require("uuid");

/***/ }),
/* 9 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a, _b;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.AwsService = void 0;
const common_1 = __webpack_require__(4);
const client_sqs_1 = __webpack_require__(10);
const config_1 = __webpack_require__(3);
const log_service_1 = __webpack_require__(5);
let AwsService = class AwsService {
    logService;
    configService;
    sqsClient;
    pollingFlags = new Map();
    runningPollers = new Map();
    credentialErrorLogged = new Set();
    constructor(logService, configService) {
        this.logService = logService;
        this.configService = configService;
        const region = this.configService.get('AWS_REGION', 'ap-southeast-1');
        const accessKeyId = this.configService.get('AWS_ACCESS_KEY_ID');
        const secretAccessKey = this.configService.get('AWS_SECRET_ACCESS_KEY');
        this.sqsClient =
            accessKeyId && secretAccessKey
                ? new client_sqs_1.SQSClient({ region, credentials: { accessKeyId, secretAccessKey } })
                : new client_sqs_1.SQSClient({ region });
    }
    async onModuleDestroy() {
        for (const queueUrl of this.pollingFlags.keys()) {
            this.pollingFlags.set(queueUrl, false);
        }
        await Promise.allSettled(this.runningPollers.values());
        this.runningPollers.clear();
        this.pollingFlags.clear();
        this.logService.log('SQS consumers disconnected');
    }
    async subscribe(queueUrl, callback) {
        if (!queueUrl) {
            throw new Error('queueUrl is required');
        }
        if (this.pollingFlags.get(queueUrl)) {
            this.logService.log(`Already subscribed to queue: ${queueUrl}`);
            return;
        }
        this.pollingFlags.set(queueUrl, true);
        const poller = this.runPollingLoop(queueUrl, callback);
        this.runningPollers.set(queueUrl, poller);
        this.logService.log(`Subscribed to SQS queue: ${queueUrl}`);
    }
    on(queueUrl, callback) {
        void this.subscribe(queueUrl, callback);
    }
    async runPollingLoop(queueUrl, callback) {
        while (this.pollingFlags.get(queueUrl)) {
            try {
                const response = await this.sqsClient.send(new client_sqs_1.ReceiveMessageCommand({
                    QueueUrl: queueUrl,
                    MaxNumberOfMessages: 1,
                    WaitTimeSeconds: 20,
                    VisibilityTimeout: 30,
                }));
                if (!response.Messages || response.Messages.length === 0) {
                    continue;
                }
                for (const message of response.Messages) {
                    let messageData;
                    try {
                        if (!message.Body) {
                            continue;
                        }
                        const body = message.Body.trim();
                        let parsedBody;
                        try {
                            parsedBody = JSON.parse(body);
                        }
                        catch (e) {
                            if (body.includes('"id":') && !body.startsWith('{')) {
                                const fixedBody = `{${body}${body.endsWith('}') ? '' : '}'}`;
                                try {
                                    parsedBody = JSON.parse(fixedBody);
                                    this.logService.warn(`Auto-fixed SQS message body missing braces for message ${message.MessageId}`);
                                }
                                catch (e2) {
                                    throw new Error(`Failed to parse SQS body even after auto-fix attempt. Original: ${body.substring(0, 100)}...`);
                                }
                            }
                            else {
                                throw e;
                            }
                        }
                        if (parsedBody && typeof parsedBody === 'object' && 'Message' in parsedBody) {
                            const innerMessage = parsedBody.Message;
                            if (typeof innerMessage === 'string') {
                                try {
                                    messageData = JSON.parse(innerMessage);
                                }
                                catch (e) {
                                    messageData = innerMessage;
                                }
                            }
                            else {
                                messageData = innerMessage;
                            }
                        }
                        else {
                            messageData = parsedBody;
                        }
                    }
                    catch (error) {
                        this.logService.error(`Failed to parse SQS message body: ${message.MessageId}. Body snippet: ${message.Body?.substring(0, 100)}`, error.stack);
                        continue;
                    }
                    try {
                        await callback(messageData);
                        if (message.ReceiptHandle) {
                            await this.sqsClient.send(new client_sqs_1.DeleteMessageCommand({
                                QueueUrl: queueUrl,
                                ReceiptHandle: message.ReceiptHandle,
                            }));
                        }
                    }
                    catch (error) {
                        this.logService.error(`Error processing message ${message.MessageId} from queue ${queueUrl}`, error?.stack);
                    }
                }
            }
            catch (error) {
                if (this.isCredentialExpiredError(error)) {
                    if (!this.credentialErrorLogged.has(queueUrl)) {
                        this.credentialErrorLogged.add(queueUrl);
                        this.logService.error(`AWS credentials expired while polling ${queueUrl}. Re-authenticate (for example: aws sso login --profile <profile>) and polling will resume automatically.`, error?.stack);
                    }
                    await this.sleep(15000);
                    continue;
                }
                this.credentialErrorLogged.delete(queueUrl);
                this.logService.error(`Error polling queue ${queueUrl}`, error?.stack);
                await this.sleep(1000);
            }
        }
        this.credentialErrorLogged.delete(queueUrl);
        this.runningPollers.delete(queueUrl);
    }
    isCredentialExpiredError(error) {
        const name = error?.name?.toLowerCase() ?? '';
        const message = error?.message?.toLowerCase() ?? '';
        return (name.includes('credentialsprovidererror') ||
            message.includes('session has expired') ||
            message.includes('security token included in the request is expired') ||
            message.includes('invalidclienttokenid') ||
            message.includes('expiredtoken'));
    }
    sleep(ms) {
        return new Promise((resolve) => setTimeout(resolve, ms));
    }
};
exports.AwsService = AwsService;
exports.AwsService = AwsService = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof log_service_1.LogService !== "undefined" && log_service_1.LogService) === "function" ? _a : Object, typeof (_b = typeof config_1.ConfigService !== "undefined" && config_1.ConfigService) === "function" ? _b : Object])
], AwsService);


/***/ }),
/* 10 */
/***/ ((module) => {

module.exports = require("@aws-sdk/client-sqs");

/***/ }),
/* 11 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.AwsQueueAdapter = void 0;
const common_1 = __webpack_require__(4);
const aws_service_1 = __webpack_require__(9);
let AwsQueueAdapter = class AwsQueueAdapter {
    awsService;
    constructor(awsService) {
        this.awsService = awsService;
    }
    async subscribe(queueName, callback) {
        await this.awsService.subscribe(queueName, async (messageBodyMessage) => {
            const normalized = typeof messageBodyMessage === 'string'
                ? messageBodyMessage
                : messageBodyMessage === null || messageBodyMessage === undefined
                    ? ''
                    : JSON.stringify(messageBodyMessage);
            await callback(normalized);
        });
    }
};
exports.AwsQueueAdapter = AwsQueueAdapter;
exports.AwsQueueAdapter = AwsQueueAdapter = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof aws_service_1.AwsService !== "undefined" && aws_service_1.AwsService) === "function" ? _a : Object])
], AwsQueueAdapter);


/***/ }),
/* 12 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.KafkaQueueAdapter = void 0;
const common_1 = __webpack_require__(4);
const kafka_service_1 = __webpack_require__(6);
let KafkaQueueAdapter = class KafkaQueueAdapter {
    kafkaService;
    constructor(kafkaService) {
        this.kafkaService = kafkaService;
    }
    async subscribe(queueName, callback) {
        await this.kafkaService.subscribe(queueName, async (payload) => {
            const value = payload.message.value?.toString();
            if (!value)
                return;
            await callback(value);
        });
    }
};
exports.KafkaQueueAdapter = KafkaQueueAdapter;
exports.KafkaQueueAdapter = KafkaQueueAdapter = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof kafka_service_1.KafkaService !== "undefined" && kafka_service_1.KafkaService) === "function" ? _a : Object])
], KafkaQueueAdapter);


/***/ }),
/* 13 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.QUEUE_SERVICE_TOKEN = void 0;
exports.QUEUE_SERVICE_TOKEN = 'QUEUE_SERVICE_TOKEN';


/***/ }),
/* 14 */
/***/ ((module) => {

module.exports = require("@nestjs/mongoose");

/***/ }),
/* 15 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.UserClientService = void 0;
const common_1 = __webpack_require__(4);
const microservices_1 = __webpack_require__(16);
const rxjs_1 = __webpack_require__(17);
let UserClientService = class UserClientService {
    client;
    userGrpcService;
    constructor(client) {
        this.client = client;
    }
    onModuleInit() {
        this.userGrpcService = this.client.getService('UserService');
    }
    async getUserById(data) {
        let user_id = data?.userId || '';
        if (data.metadata) {
            user_id = this.extractUserMetadata(data.metadata).userId;
        }
        const res = await (0, rxjs_1.firstValueFrom)(this.userGrpcService.getById({ userId: user_id }));
        return res.data;
    }
    async getListUsers(params) {
        const res = await (0, rxjs_1.firstValueFrom)(this.userGrpcService.listUsers({ userIds: params.userIds, limit: params.limit, page: params.page }));
        return res.data;
    }
    extractUserMetadata(metadata) {
        return {
            userId: metadata.get('userId')?.[0] || '',
            userRole: metadata.get('userRole')?.[0] || '',
        };
    }
};
exports.UserClientService = UserClientService;
exports.UserClientService = UserClientService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, common_1.Inject)('USER_PACKAGE')),
    __metadata("design:paramtypes", [typeof (_a = typeof microservices_1.ClientGrpc !== "undefined" && microservices_1.ClientGrpc) === "function" ? _a : Object])
], UserClientService);


/***/ }),
/* 16 */
/***/ ((module) => {

module.exports = require("@nestjs/microservices");

/***/ }),
/* 17 */
/***/ ((module) => {

module.exports = require("rxjs");

/***/ }),
/* 18 */
/***/ ((module) => {

module.exports = require("path");

/***/ }),
/* 19 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.ProjectClientService = void 0;
const common_1 = __webpack_require__(4);
const microservices_1 = __webpack_require__(16);
const rxjs_1 = __webpack_require__(17);
let ProjectClientService = class ProjectClientService {
    client;
    projectGrpcService;
    constructor(client) {
        this.client = client;
    }
    onModuleInit() {
        this.projectGrpcService = this.client.getService('ProjectService');
    }
    async getListProjects(param) {
        const res = await (0, rxjs_1.firstValueFrom)(this.projectGrpcService.listProjects({ projectIds: param.projectIds, limit: param.limit, page: param.page, kw: '', sort: '' }));
        return res.data;
    }
};
exports.ProjectClientService = ProjectClientService;
exports.ProjectClientService = ProjectClientService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, common_1.Inject)('PROJECT_PACKAGE')),
    __metadata("design:paramtypes", [typeof (_a = typeof microservices_1.ClientGrpc !== "undefined" && microservices_1.ClientGrpc) === "function" ? _a : Object])
], ProjectClientService);


/***/ }),
/* 20 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.SprintClientService = void 0;
const common_1 = __webpack_require__(4);
const microservices_1 = __webpack_require__(16);
const rxjs_1 = __webpack_require__(17);
let SprintClientService = class SprintClientService {
    client;
    sprintGrpcService;
    constructor(client) {
        this.client = client;
    }
    onModuleInit() {
        this.sprintGrpcService = this.client.getService('SprintService');
    }
    async getListSprints(params) {
        const res = await (0, rxjs_1.firstValueFrom)(this.sprintGrpcService.listSprints({ sprintIds: params.sprintIds, limit: params.limit, page: params.page }));
        return res.data;
    }
};
exports.SprintClientService = SprintClientService;
exports.SprintClientService = SprintClientService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, common_1.Inject)('SPRINT_PACKAGE')),
    __metadata("design:paramtypes", [typeof (_a = typeof microservices_1.ClientGrpc !== "undefined" && microservices_1.ClientGrpc) === "function" ? _a : Object])
], SprintClientService);


/***/ }),
/* 21 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.IssueClientService = void 0;
const common_1 = __webpack_require__(4);
const microservices_1 = __webpack_require__(16);
const rxjs_1 = __webpack_require__(17);
let IssueClientService = class IssueClientService {
    client;
    issueGrpcService;
    constructor(client) {
        this.client = client;
    }
    onModuleInit() {
        this.issueGrpcService = this.client.getService('IssueService');
    }
    async getListIssues(params) {
        const res = await (0, rxjs_1.firstValueFrom)(this.issueGrpcService.listIssues({
            page: params.page,
            limit: params.limit,
            assigneeIds: [],
            columnIds: [],
            issueIds: params.issueIds,
            sprintIds: [],
            projectId: '',
            types: [],
            priorities: [],
            teamIds: [],
            parentIds: [],
        }));
        return res.data;
    }
};
exports.IssueClientService = IssueClientService;
exports.IssueClientService = IssueClientService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, common_1.Inject)('ISSUE_PACKAGE')),
    __metadata("design:paramtypes", [typeof (_a = typeof microservices_1.ClientGrpc !== "undefined" && microservices_1.ClientGrpc) === "function" ? _a : Object])
], IssueClientService);


/***/ }),
/* 22 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a, _b;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.CassandraService = void 0;
const common_1 = __webpack_require__(4);
const config_1 = __webpack_require__(3);
const cassandra_driver_1 = __webpack_require__(23);
const log_service_1 = __webpack_require__(5);
let CassandraService = class CassandraService {
    configService;
    logService;
    client;
    constructor(configService, logService) {
        this.configService = configService;
        this.logService = logService;
        const contactPoints = this.configService.get('CASSANDRA_CONTACT_POINTS', 'localhost').split(',');
        const localDataCenter = this.configService.get('CASSANDRA_LOCAL_DATA_CENTER', 'datacenter1');
        const keyspace = this.configService.get('CASSANDRA_KEYSPACE', 'taskflow');
        this.client = new cassandra_driver_1.Client({
            contactPoints,
            localDataCenter,
            keyspace,
        });
    }
    async onModuleInit() {
        try {
            await this.client.connect();
            this.logService.log('Cassandra client connected');
        }
        catch (error) {
            this.logService.error('Failed to connect to Cassandra', error.stack);
            throw error;
        }
    }
    async onModuleDestroy() {
        try {
            await this.client.shutdown();
            this.logService.log('Cassandra client disconnected');
        }
        catch (error) {
            this.logService.error('Failed to disconnect from Cassandra', error.stack);
        }
    }
    getClient() {
        return this.client;
    }
    async executeQuery(query, params, options) {
        try {
            const result = await this.client.execute(query, params, {
                prepare: true,
                ...options,
            });
            this.logService.log(`Executed query: ${query}`);
            return result;
        }
        catch (error) {
            this.logService.error(`Error executing Cassandra query: ${query}`, error.stack);
            throw error;
        }
    }
};
exports.CassandraService = CassandraService;
exports.CassandraService = CassandraService = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof config_1.ConfigService !== "undefined" && config_1.ConfigService) === "function" ? _a : Object, typeof (_b = typeof log_service_1.LogService !== "undefined" && log_service_1.LogService) === "function" ? _b : Object])
], CassandraService);


/***/ }),
/* 23 */
/***/ ((module) => {

module.exports = require("cassandra-driver");

/***/ }),
/* 24 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var GlobalHandleErrorInterceptor_1;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.GlobalHandleErrorInterceptor = void 0;
const common_1 = __webpack_require__(4);
const rxjs_1 = __webpack_require__(17);
const microservices_1 = __webpack_require__(16);
const grpc_js_1 = __webpack_require__(25);
let GlobalHandleErrorInterceptor = GlobalHandleErrorInterceptor_1 = class GlobalHandleErrorInterceptor {
    logger = new common_1.Logger(GlobalHandleErrorInterceptor_1.name);
    intercept(context, next) {
        return next.handle().pipe((0, rxjs_1.catchError)((error) => {
            const handler = context.getHandler().name;
            this.logger.error(`[gRPC Error] ${handler}: ${error.message || error}`);
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INTERNAL,
                message: error.message || 'Internal server error',
            });
        }));
    }
};
exports.GlobalHandleErrorInterceptor = GlobalHandleErrorInterceptor;
exports.GlobalHandleErrorInterceptor = GlobalHandleErrorInterceptor = GlobalHandleErrorInterceptor_1 = __decorate([
    (0, common_1.Injectable)()
], GlobalHandleErrorInterceptor);


/***/ }),
/* 25 */
/***/ ((module) => {

module.exports = require("@grpc/grpc-js");

/***/ }),
/* 26 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.GrpcAuthInterceptor = void 0;
const common_1 = __webpack_require__(4);
const microservices_1 = __webpack_require__(16);
const constants_1 = __webpack_require__(27);
const jwt_decode_1 = __webpack_require__(28);
let GrpcAuthInterceptor = class GrpcAuthInterceptor {
    intercept(context, next) {
        const metadata = context.getArgByIndex(1);
        const rawCookie = metadata.get('cookie')?.[0];
        const cookieHeader = Buffer.isBuffer(rawCookie) ? rawCookie.toString() : rawCookie;
        const token = this.getTokenFromCookie(cookieHeader);
        if (token) {
            try {
                const payload = (0, jwt_decode_1.jwtDecode)(token);
                metadata.add('userId', String(payload.userId));
                metadata.add('userRole', String(payload.role));
            }
            catch (error) {
                throw new microservices_1.RpcException({
                    code: constants_1.Status.UNAUTHENTICATED,
                    message: `Token verification failed: ${error.message}`,
                });
            }
        }
        return next.handle();
    }
    getTokenFromCookie(cookieHeader) {
        if (!cookieHeader)
            return null;
        const cookies = cookieHeader.split(';');
        for (const cookie of cookies) {
            const trimmed = cookie.trim();
            if (trimmed.startsWith('token=')) {
                return trimmed.substring('token='.length);
            }
        }
        return null;
    }
};
exports.GrpcAuthInterceptor = GrpcAuthInterceptor;
exports.GrpcAuthInterceptor = GrpcAuthInterceptor = __decorate([
    (0, common_1.Injectable)()
], GrpcAuthInterceptor);


/***/ }),
/* 27 */
/***/ ((module) => {

module.exports = require("@grpc/grpc-js/build/src/constants");

/***/ }),
/* 28 */
/***/ ((module) => {

module.exports = require("jwt-decode");

/***/ }),
/* 29 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.HttpAuthInterceptor = void 0;
const common_1 = __webpack_require__(4);
const jwt_decode_1 = __webpack_require__(28);
let HttpAuthInterceptor = class HttpAuthInterceptor {
    intercept(context, next) {
        const request = context.switchToHttp().getRequest();
        const cookieHeader = request.headers.cookie;
        const token = this.getTokenFromCookie(cookieHeader);
        if (!token) {
            throw new common_1.UnauthorizedException('No token found in cookies');
        }
        try {
            const payload = (0, jwt_decode_1.jwtDecode)(token);
            request.user = {
                userId: payload.userId,
                role: payload.role,
            };
        }
        catch (error) {
            throw new common_1.UnauthorizedException(`Invalid token: ${error.message}`);
        }
        return next.handle();
    }
    getTokenFromCookie(cookieHeader) {
        if (!cookieHeader)
            return null;
        const cookies = cookieHeader.split(';');
        for (const cookie of cookies) {
            const trimmed = cookie.trim();
            if (trimmed.startsWith('token=')) {
                return trimmed.substring('token='.length);
            }
        }
        return null;
    }
};
exports.HttpAuthInterceptor = HttpAuthInterceptor;
exports.HttpAuthInterceptor = HttpAuthInterceptor = __decorate([
    (0, common_1.Injectable)()
], HttpAuthInterceptor);


/***/ }),
/* 30 */
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.UserMetadata = void 0;
const common_1 = __webpack_require__(4);
exports.UserMetadata = (0, common_1.createParamDecorator)((_, ctx) => {
    const metadata = ctx.switchToRpc().getContext();
    const userId = metadata.get('userId')?.[0];
    const userRole = metadata.get('userRole')?.[0];
    return {
        userId,
        userRole,
    };
});


/***/ }),
/* 31 */
/***/ ((module) => {

module.exports = require("@nestjs/core");

/***/ }),
/* 32 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.TransformResponseInterceptor = void 0;
const common_1 = __webpack_require__(4);
const convert_utils_1 = __webpack_require__(33);
const operators_1 = __webpack_require__(34);
let TransformResponseInterceptor = class TransformResponseInterceptor {
    intercept(context, next) {
        return next.handle().pipe((0, operators_1.map)((data) => convert_utils_1.default.convertToSnakeCase(data)));
    }
};
exports.TransformResponseInterceptor = TransformResponseInterceptor;
exports.TransformResponseInterceptor = TransformResponseInterceptor = __decorate([
    (0, common_1.Injectable)()
], TransformResponseInterceptor);


/***/ }),
/* 33 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
const toSnakeCase = (str) => {
    return str.replace(/([A-Z])/g, '_$1').toLowerCase();
};
const convertToSnakeCase = (obj) => {
    if (Array.isArray(obj)) {
        return obj.map((item) => convertToSnakeCase(item));
    }
    if (obj !== null && typeof obj === 'object') {
        return Object.entries(obj).reduce((acc, [key, value]) => {
            const newKey = toSnakeCase(key);
            acc[newKey] = convertToSnakeCase(value);
            return acc;
        }, {});
    }
    return obj;
};
const convert = {
    toSnakeCase,
    convertToSnakeCase,
};
exports["default"] = convert;


/***/ }),
/* 34 */
/***/ ((module) => {

module.exports = require("rxjs/operators");

/***/ }),
/* 35 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.NotificationModule = void 0;
const common_1 = __webpack_require__(4);
const core_1 = __webpack_require__(1);
const config_1 = __webpack_require__(3);
const notification_service_1 = __webpack_require__(36);
const notification_subcriber_service_1 = __webpack_require__(42);
const dynamodDBNotification_repo_1 = __webpack_require__(44);
const mongoose_1 = __webpack_require__(14);
const notification_repo_interface_1 = __webpack_require__(38);
const notification_schema_1 = __webpack_require__(48);
const notification_controller_1 = __webpack_require__(49);
const health_controller_1 = __webpack_require__(50);
const notification_websocket_1 = __webpack_require__(39);
const mail_service_1 = __webpack_require__(51);
const mail_sender_interface_1 = __webpack_require__(52);
const mail_sender_repo_1 = __webpack_require__(53);
let NotificationModule = class NotificationModule {
};
exports.NotificationModule = NotificationModule;
exports.NotificationModule = NotificationModule = __decorate([
    (0, common_1.Module)({
        imports: [
            config_1.ConfigModule.forRoot({
                isGlobal: true,
                envFilePath: ['.env'],
            }),
            core_1.CoreModule,
            mongoose_1.MongooseModule.forFeature([{ name: notification_schema_1.INotification.name, schema: notification_schema_1.NotificationSchema }]),
        ],
        controllers: [notification_controller_1.NotificationController, health_controller_1.HealthController],
        providers: [
            notification_websocket_1.NotificationGateway,
            mail_service_1.MailService,
            notification_service_1.NotificationService,
            notification_subcriber_service_1.NotificationSubscriberService,
            notification_websocket_1.NotificationEmitterService,
            {
                provide: notification_repo_interface_1.INotificationRepo,
                useClass: dynamodDBNotification_repo_1.DynamoDBNotificationRepo,
            },
            {
                provide: mail_sender_interface_1.IMailSender,
                useClass: mail_sender_repo_1.MailSenderRepo,
            },
        ],
    })
], NotificationModule);


/***/ }),
/* 36 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var NotificationService_1;
var _a, _b, _c, _d, _e, _f;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.NotificationService = void 0;
const common_1 = __webpack_require__(4);
const notification_1 = __webpack_require__(37);
const notification_repo_interface_1 = __webpack_require__(38);
const microservices_1 = __webpack_require__(16);
const grpc_js_1 = __webpack_require__(25);
const core_1 = __webpack_require__(1);
const notification_websocket_1 = __webpack_require__(39);
let NotificationService = NotificationService_1 = class NotificationService {
    notificationRepo;
    projectClientService;
    sprintCLientService;
    issueClientService;
    userClientService;
    notiEmitter;
    logger = new common_1.Logger(NotificationService_1.name);
    constructor(notificationRepo, projectClientService, sprintCLientService, issueClientService, userClientService, notiEmitter) {
        this.notificationRepo = notificationRepo;
        this.projectClientService = projectClientService;
        this.sprintCLientService = sprintCLientService;
        this.issueClientService = issueClientService;
        this.userClientService = userClientService;
        this.notiEmitter = notiEmitter;
    }
    async createNotification(data) {
        const refType = this.getReferenceTypeByNotification(data.type);
        const noti = await this.notificationRepo.create({
            recipientId: data.recipientId,
            actorId: data.actorId,
            type: data.type,
            referenceId: data.referenceId,
            referenceType: refType,
            content: data?.content || '',
            isRead: false,
            createdAt: new Date(),
        });
        await this.notiEmitter.sendToUser(data.recipientId);
        return noti;
    }
    async listNotifications(params) {
        const [notiDomain, totalCount] = await Promise.all([this.notificationRepo.listAll(params), this.notificationRepo.countAll(params)]);
        const issueIds = [];
        const projectIds = [];
        const sprintIds = [];
        const actorIds = notiDomain.map((noti) => noti?.actorId).filter(Boolean);
        notiDomain.forEach((noti) => {
            if (noti.referenceType == notification_1.ReferenceType.ISSUE && noti.referenceId) {
                issueIds.push(noti.referenceId);
            }
            if (noti.referenceType == notification_1.ReferenceType.PROJECT && noti.referenceId) {
                projectIds.push(noti.referenceId);
            }
            if (noti.referenceType == notification_1.ReferenceType.SPRINT && noti.referenceId) {
                sprintIds.push(noti.referenceId);
            }
        });
        try {
            const [projects, sprints, issues, receivers, actors] = await Promise.all([
                this.projectClientService.getListProjects({ projectIds: projectIds, limit: params.limit || 100, page: params.page || 1 }),
                this.sprintCLientService.getListSprints({ sprintIds: sprintIds, limit: params.limit || 100, page: params.page || 1 }),
                this.issueClientService.getListIssues({ issueIds: issueIds, limit: params.limit || 100, page: params.page || 1 }),
                this.userClientService.getListUsers({ userIds: notiDomain.map((noti) => noti.recipientId), limit: params.limit || 100, page: params.page || 1 }),
                this.userClientService.getListUsers({ userIds: actorIds, limit: params.limit || 100, page: params.page || 1 }),
            ]);
            const projectsMap = new Map();
            (projects || []).forEach((project) => {
                projectsMap.set(project.id, project);
            });
            const sprintsMap = new Map();
            (sprints || []).forEach((sprint) => {
                sprintsMap.set(sprint.id, sprint);
            });
            const issuesMaps = new Map();
            (issues || []).forEach((issue) => {
                issuesMaps.set(issue.id, issue);
            });
            const receiverMaps = new Map();
            (receivers || []).forEach((receiver) => {
                receiverMaps.set(receiver.id, receiver);
            });
            const actorMaps = new Map();
            (actors || []).forEach((actor) => {
                actorMaps.set(actor.id, actor);
            });
            notiDomain.forEach((noti) => {
                if (noti.referenceType === notification_1.ReferenceType.PROJECT && noti.referenceId) {
                    const project = projectsMap.get(noti.referenceId);
                    if (project) {
                        noti.referenceData = project;
                    }
                }
                if (noti.referenceType === notification_1.ReferenceType.SPRINT && noti.referenceId) {
                    const sprint = sprintsMap.get(noti.referenceId);
                    if (sprint) {
                        noti.referenceData = sprint;
                    }
                }
                if (noti.referenceType === notification_1.ReferenceType.ISSUE && noti.referenceId) {
                    const issue = issuesMaps.get(noti.referenceId);
                    if (issue) {
                        noti.referenceData = issue;
                    }
                }
                const receiver = receiverMaps.get(noti.recipientId);
                if (receiver) {
                    noti.recipient = receiver;
                }
                if (noti.actorId) {
                    const actor = actorMaps.get(noti.actorId);
                    noti.actor = actor;
                }
            });
        }
        catch (err) {
            const message = err instanceof Error ? err.message : String(err);
            this.logger.warn(`listNotifications: gRPC enrichment skipped (MAIN_SERVICE unreachable or error): ${message}`);
        }
        return {
            data: notiDomain,
            totalCount: totalCount,
        };
    }
    async updateNotification(data) {
        const noti = await this.notificationRepo.update(data);
        return noti;
    }
    async bulkUpdateNotification(data) {
        return await this.notificationRepo.bulkUpdate(data);
    }
    getReferenceTypeByNotification(type) {
        switch (type) {
            case notification_1.NotificationType.ASSIGNMENT:
            case notification_1.NotificationType.MENTION:
            case notification_1.NotificationType.COMMENT:
            case notification_1.NotificationType.STATUS_UPDATE:
            case notification_1.NotificationType.DUE_DATE_REMINDER:
                return notification_1.ReferenceType.ISSUE;
            case notification_1.NotificationType.SPRINT_STARTED:
                return notification_1.ReferenceType.SPRINT;
            case notification_1.NotificationType.PROJECT_INVITATION:
            case notification_1.NotificationType.PROJECT_ADDED:
                return notification_1.ReferenceType.PROJECT;
            case notification_1.NotificationType.REACTION:
                return notification_1.ReferenceType.COMMENT;
            case notification_1.NotificationType.PROJECT_TEAM_ADDED:
                return notification_1.ReferenceType.PROJECT_MEMBER;
            case notification_1.NotificationType.SYSTEM_ALERT:
                return notification_1.ReferenceType.SYSTEM;
            default:
                return notification_1.ReferenceType.SYSTEM;
        }
    }
    ValidateNotificationType(type) {
        if (!type || !Object.values(notification_1.NotificationType).includes(type)) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INVALID_ARGUMENT,
                message: 'Invalid notification type',
            });
        }
        return type;
    }
};
exports.NotificationService = NotificationService;
exports.NotificationService = NotificationService = NotificationService_1 = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof notification_repo_interface_1.INotificationRepo !== "undefined" && notification_repo_interface_1.INotificationRepo) === "function" ? _a : Object, typeof (_b = typeof core_1.ProjectClientService !== "undefined" && core_1.ProjectClientService) === "function" ? _b : Object, typeof (_c = typeof core_1.SprintClientService !== "undefined" && core_1.SprintClientService) === "function" ? _c : Object, typeof (_d = typeof core_1.IssueClientService !== "undefined" && core_1.IssueClientService) === "function" ? _d : Object, typeof (_e = typeof core_1.UserClientService !== "undefined" && core_1.UserClientService) === "function" ? _e : Object, typeof (_f = typeof notification_websocket_1.NotificationEmitterService !== "undefined" && notification_websocket_1.NotificationEmitterService) === "function" ? _f : Object])
], NotificationService);


/***/ }),
/* 37 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.ReferenceType = exports.NotificationType = void 0;
var NotificationType;
(function (NotificationType) {
    NotificationType["ASSIGNMENT"] = "ASSIGNMENT";
    NotificationType["MENTION"] = "MENTION";
    NotificationType["COMMENT"] = "COMMENT";
    NotificationType["STATUS_UPDATE"] = "STATUS_UPDATE";
    NotificationType["DUE_DATE_REMINDER"] = "DUE_DATE_REMINDER";
    NotificationType["PROJECT_INVITATION"] = "PROJECT_INVITATION";
    NotificationType["REACTION"] = "REACTION";
    NotificationType["SYSTEM_ALERT"] = "SYSTEM_ALERT";
    NotificationType["SPRINT_STARTED"] = "SPRINT_STARTED";
    NotificationType["PROJECT_ADDED"] = "PROJECT_ADDED";
    NotificationType["PROJECT_TEAM_ADDED"] = "PROJECT_TEAM_ADDED";
})(NotificationType || (exports.NotificationType = NotificationType = {}));
var ReferenceType;
(function (ReferenceType) {
    ReferenceType["ISSUE"] = "issue";
    ReferenceType["PROJECT"] = "project";
    ReferenceType["COMMENT"] = "comment";
    ReferenceType["SPRINT"] = "sprint";
    ReferenceType["SYSTEM"] = "system";
    ReferenceType["PROJECT_MEMBER"] = "project_member";
})(ReferenceType || (exports.ReferenceType = ReferenceType = {}));


/***/ }),
/* 38 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.INotificationRepo = void 0;
class INotificationRepo {
}
exports.INotificationRepo = INotificationRepo;


/***/ }),
/* 39 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a, _b;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.NotificationGateway = exports.NotificationEmitterService = void 0;
const common_1 = __webpack_require__(4);
const websockets_1 = __webpack_require__(40);
const notification_service_1 = __webpack_require__(36);
const socket_io_1 = __webpack_require__(41);
const convert_utils_1 = __webpack_require__(33);
let NotificationEmitterService = class NotificationEmitterService {
    notificationService;
    server;
    constructor(notificationService) {
        this.notificationService = notificationService;
    }
    setServer(server) {
        this.server = server;
    }
    broadcast(event, data) {
        if (this.server) {
            this.server.emit(event, data);
        }
    }
    async sendToUser(userId) {
        if (this.server) {
            const notis = await this.notificationService.listNotifications({
                page: 1,
                limit: 100,
                userId: userId,
            });
            this.server.to(userId).emit('refresh-list', convert_utils_1.default.convertToSnakeCase({
                notifications: notis.data,
                total: notis.totalCount,
            }));
        }
    }
};
exports.NotificationEmitterService = NotificationEmitterService;
exports.NotificationEmitterService = NotificationEmitterService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, common_1.Inject)((0, common_1.forwardRef)(() => notification_service_1.NotificationService))),
    __metadata("design:paramtypes", [typeof (_a = typeof notification_service_1.NotificationService !== "undefined" && notification_service_1.NotificationService) === "function" ? _a : Object])
], NotificationEmitterService);
let NotificationGateway = class NotificationGateway {
    emitter;
    server;
    constructor(emitter) {
        this.emitter = emitter;
    }
    afterInit(server) {
        this.emitter.setServer(server);
    }
    async handleConnection(socket) {
        const userId = socket.handshake.query.userId;
        if (userId && typeof userId === 'string') {
            await socket.join(userId);
            common_1.Logger.log(`User ${userId} connected`);
            await this.emitter.sendToUser(userId);
        }
        else {
            socket.disconnect();
            common_1.Logger.warn(`Socket disconnected due to missing userId`);
        }
    }
    handleDisconnect(socket) {
        common_1.Logger.log(`Client disconnected: ${socket.id}`);
    }
};
exports.NotificationGateway = NotificationGateway;
__decorate([
    (0, websockets_1.WebSocketServer)(),
    __metadata("design:type", typeof (_b = typeof socket_io_1.Server !== "undefined" && socket_io_1.Server) === "function" ? _b : Object)
], NotificationGateway.prototype, "server", void 0);
exports.NotificationGateway = NotificationGateway = __decorate([
    (0, common_1.Injectable)(),
    (0, websockets_1.WebSocketGateway)({
        path: '/notification-service/socket.io',
        cors: {
            origin: ['http://localhost:5173', 'http://localhost:4173', 'https://frontend.taskkfloww.shop'],
            credentials: true,
        },
    }),
    __metadata("design:paramtypes", [NotificationEmitterService])
], NotificationGateway);


/***/ }),
/* 40 */
/***/ ((module) => {

module.exports = require("@nestjs/websockets");

/***/ }),
/* 41 */
/***/ ((module) => {

module.exports = require("socket.io");

/***/ }),
/* 42 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a, _b, _c;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.NotificationSubscriberService = void 0;
const core_1 = __webpack_require__(1);
const common_1 = __webpack_require__(4);
const notification_service_1 = __webpack_require__(36);
const validate_utils_1 = __webpack_require__(43);
const config_1 = __webpack_require__(3);
let NotificationSubscriberService = class NotificationSubscriberService {
    queueService;
    notificationService;
    configService;
    constructor(queueService, notificationService, configService) {
        this.queueService = queueService;
        this.notificationService = notificationService;
        this.configService = configService;
    }
    async onModuleInit() {
        const queueName = this.configService.get('NOTIFICATION_QUEUE_NAME') || 'https://sqs.ap-southeast-1.amazonaws.com/017263836577/notifications.fifo';
        await this.queueService.subscribe(queueName, this.handleNotificationReceiver.bind(this));
    }
    async handleNotificationReceiver(queueMessage) {
        try {
            await this.handleCreateNotification(queueMessage);
        }
        catch (err) {
            console.error('❌ [NOTIFICATION_TOPIC] Failed to process notification message:', err);
        }
    }
    async handleCreateNotification(queueMessage) {
        const kafkaMessage = JSON.parse(queueMessage);
        const { isValid, message, data } = validate_utils_1.default.validateRequiredFields(kafkaMessage);
        if (!isValid || !data) {
            console.error('❌ Missing field:', message);
            return;
        }
        const type = this.notificationService.ValidateNotificationType(data.type);
        const notification = {
            recipientId: data.recipientId,
            type: type,
            content: ``,
            isRead: false,
            createdAt: new Date(),
            actorId: data.actorId,
            referenceId: data.issueId,
        };
        await this.notificationService.createNotification(notification);
    }
};
exports.NotificationSubscriberService = NotificationSubscriberService;
exports.NotificationSubscriberService = NotificationSubscriberService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, common_1.Inject)(core_1.QUEUE_SERVICE_TOKEN)),
    __metadata("design:paramtypes", [typeof (_a = typeof core_1.IQueueService !== "undefined" && core_1.IQueueService) === "function" ? _a : Object, typeof (_b = typeof notification_service_1.NotificationService !== "undefined" && notification_service_1.NotificationService) === "function" ? _b : Object, typeof (_c = typeof config_1.ConfigService !== "undefined" && config_1.ConfigService) === "function" ? _c : Object])
], NotificationSubscriberService);


/***/ }),
/* 43 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
const validator = {
    validateRequiredFields(message) {
        if (!message.data) {
            return {
                isValid: false,
                message: `Missing data for ${message.id} event`,
            };
        }
        const requiredFields = Object.keys(message.data);
        const missingFields = requiredFields.filter((field) => {
            const value = message.data[field];
            return value === undefined || value === null;
        });
        if (missingFields.length > 0) {
            return {
                isValid: false,
                message: `Missing required fields for ${message.id} event: ${missingFields.join(', ')}`,
                missingFields: missingFields,
            };
        }
        return {
            isValid: true,
            message: 'valid',
            data: message.data,
        };
    },
};
exports["default"] = validator;


/***/ }),
/* 44 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.DynamoDBNotificationRepo = void 0;
const common_1 = __webpack_require__(4);
const config_1 = __webpack_require__(3);
const client_dynamodb_1 = __webpack_require__(45);
const lib_dynamodb_1 = __webpack_require__(46);
const mapper_1 = __webpack_require__(47);
const uuid_1 = __webpack_require__(8);
let DynamoDBNotificationRepo = class DynamoDBNotificationRepo {
    configService;
    docClient;
    tableName = 'taskflow';
    constructor(configService) {
        this.configService = configService;
        const region = this.configService.get('AWS_REGION', 'ap-southeast-1');
        const accessKeyId = this.configService.get('AWS_ACCESS_KEY_ID');
        const secretAccessKey = this.configService.get('AWS_SECRET_ACCESS_KEY');
        const endpoint = this.configService.get('DYNAMODB_ENDPOINT');
        const clientConfig = { region };
        if (accessKeyId && secretAccessKey) {
            clientConfig.credentials = { accessKeyId, secretAccessKey };
        }
        if (endpoint) {
            clientConfig.endpoint = endpoint;
        }
        const client = new client_dynamodb_1.DynamoDBClient(clientConfig);
        this.docClient = lib_dynamodb_1.DynamoDBDocumentClient.from(client, {
            marshallOptions: {
                removeUndefinedValues: true,
            },
        });
    }
    mapToMongoSchemaShape(item) {
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
    async create(notification) {
        const notiId = notification.id || (0, uuid_1.v4)();
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
        await this.docClient.send(new lib_dynamodb_1.PutCommand({
            TableName: this.tableName,
            Item: item,
        }));
        notification.id = notiId;
        return notification;
    }
    async countAll(params) {
        if (!params?.userId) {
            return 0;
        }
        const queryParams = {
            TableName: this.tableName,
            KeyConditionExpression: 'recipentId = :recipientId',
            ExpressionAttributeValues: {
                ':recipientId': params.userId,
            },
        };
        const filterExpressions = [];
        if (params.projectId) {
            filterExpressions.push('referenceType = :refType AND referenceId = :refId');
            queryParams.ExpressionAttributeValues[':refType'] = 'project';
            queryParams.ExpressionAttributeValues[':refId'] = params.projectId;
        }
        else if (params.sprintId) {
            filterExpressions.push('referenceType = :refType AND referenceId = :refId');
            queryParams.ExpressionAttributeValues[':refType'] = 'sprint';
            queryParams.ExpressionAttributeValues[':refId'] = params.sprintId;
        }
        if (filterExpressions.length > 0) {
            queryParams.FilterExpression = filterExpressions.join(' AND ');
        }
        const result = await this.docClient.send(new lib_dynamodb_1.QueryCommand(queryParams));
        return result.Count ?? 0;
    }
    async listAll(params) {
        if (!params.userId) {
            return [];
        }
        const queryParams = {
            TableName: this.tableName,
            KeyConditionExpression: 'recipentId = :recipientId',
            ExpressionAttributeValues: {
                ':recipientId': params.userId,
            },
            ScanIndexForward: false,
        };
        const filterExpressions = [];
        if (params.projectId) {
            filterExpressions.push('referenceType = :refType AND referenceId = :refId');
            queryParams.ExpressionAttributeValues[':refType'] = 'project';
            queryParams.ExpressionAttributeValues[':refId'] = params.projectId;
        }
        else if (params.sprintId) {
            filterExpressions.push('referenceType = :refType AND referenceId = :refId');
            queryParams.ExpressionAttributeValues[':refType'] = 'sprint';
            queryParams.ExpressionAttributeValues[':refId'] = params.sprintId;
        }
        if (filterExpressions.length > 0) {
            queryParams.FilterExpression = filterExpressions.join(' AND ');
        }
        const result = await this.docClient.send(new lib_dynamodb_1.QueryCommand(queryParams));
        const items = result.Items ?? [];
        const page = params.page ?? 1;
        const limit = params.limit ?? 10;
        const skip = (page - 1) * limit;
        const paginatedItems = items.slice(skip, skip + limit);
        const mongoShapeItems = paginatedItems.map((item) => this.mapToMongoSchemaShape(item));
        return mapper_1.NotificationMapper.toDomainList(mongoShapeItems);
    }
    async update(params) {
        const { notiId, isRead } = params;
        const scanResult = await this.docClient.send(new lib_dynamodb_1.ScanCommand({
            TableName: this.tableName,
            FilterExpression: 'id = :id',
            ExpressionAttributeValues: {
                ':id': notiId,
            },
        }));
        const item = scanResult.Items?.[0];
        if (!item) {
            throw new Error(`Notification with ID ${notiId} not found.`);
        }
        const updatedResult = await this.docClient.send(new lib_dynamodb_1.UpdateCommand({
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
        }));
        const updatedItem = updatedResult.Attributes;
        return mapper_1.NotificationMapper.toDomain(this.mapToMongoSchemaShape(updatedItem));
    }
    async bulkUpdate(params) {
        const { userId, isRead } = params;
        const queryResult = await this.docClient.send(new lib_dynamodb_1.QueryCommand({
            TableName: this.tableName,
            KeyConditionExpression: 'recipentId = :recipientId',
            ExpressionAttributeValues: {
                ':recipientId': userId,
            },
        }));
        const items = queryResult.Items ?? [];
        if (items.length === 0) {
            throw new Error(`Notification not found with this user`);
        }
        const itemsToUpdate = items.filter((item) => item.isRead !== isRead);
        if (itemsToUpdate.length === 0) {
            return { success: true };
        }
        await Promise.all(itemsToUpdate.map((item) => this.docClient.send(new lib_dynamodb_1.UpdateCommand({
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
        }))));
        return { success: true };
    }
};
exports.DynamoDBNotificationRepo = DynamoDBNotificationRepo;
exports.DynamoDBNotificationRepo = DynamoDBNotificationRepo = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof config_1.ConfigService !== "undefined" && config_1.ConfigService) === "function" ? _a : Object])
], DynamoDBNotificationRepo);


/***/ }),
/* 45 */
/***/ ((module) => {

module.exports = require("@aws-sdk/client-dynamodb");

/***/ }),
/* 46 */
/***/ ((module) => {

module.exports = require("@aws-sdk/lib-dynamodb");

/***/ }),
/* 47 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.NotificationMapper = void 0;
class NotificationMapper {
    static toDomain(entity) {
        return {
            id: entity._id.toString(),
            recipientId: entity.recipientId?.toString(),
            actorId: entity.actorId?.toString(),
            type: entity.type,
            referenceId: entity.referenceId,
            referenceType: entity.referenceType,
            content: entity.content,
            isRead: entity.isRead,
            createdAt: entity.createdAt.toISOString(),
        };
    }
    static toDomainList(entities) {
        return entities.map((entity) => this.toDomain(entity));
    }
}
exports.NotificationMapper = NotificationMapper;


/***/ }),
/* 48 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.NotificationSchema = exports.INotification = void 0;
const mongoose_1 = __webpack_require__(14);
let INotification = class INotification {
    _id;
    recipientId;
    actorId;
    type;
    referenceId;
    referenceType;
    content;
    isRead;
    createdAt;
};
exports.INotification = INotification;
__decorate([
    (0, mongoose_1.Prop)({ required: true }),
    __metadata("design:type", String)
], INotification.prototype, "recipientId", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", String)
], INotification.prototype, "actorId", void 0);
__decorate([
    (0, mongoose_1.Prop)({ required: true }),
    __metadata("design:type", String)
], INotification.prototype, "type", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", String)
], INotification.prototype, "referenceId", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", String)
], INotification.prototype, "referenceType", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", String)
], INotification.prototype, "content", void 0);
__decorate([
    (0, mongoose_1.Prop)({ default: false }),
    __metadata("design:type", Boolean)
], INotification.prototype, "isRead", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", typeof (_a = typeof Date !== "undefined" && Date) === "function" ? _a : Object)
], INotification.prototype, "createdAt", void 0);
exports.INotification = INotification = __decorate([
    (0, mongoose_1.Schema)({ timestamps: { createdAt: 'createdAt' }, collection: 'notifications' })
], INotification);
exports.NotificationSchema = mongoose_1.SchemaFactory.createForClass(INotification);


/***/ }),
/* 49 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.NotificationController = void 0;
const common_1 = __webpack_require__(4);
const notification_service_1 = __webpack_require__(36);
let NotificationController = class NotificationController {
    notificationService;
    constructor(notificationService) {
        this.notificationService = notificationService;
    }
    async sendNotification(data) {
        const type = this.notificationService.ValidateNotificationType(data.type);
        const noti = await this.notificationService.createNotification({
            content: data.content || '',
            createdAt: new Date(),
            isRead: false,
            recipientId: data.recipientId,
            type: type,
            actorId: data.actorId,
            referenceId: data.recipientId,
        });
        return {
            status: 'success',
            message: 'Notification sent!',
            data: noti,
        };
    }
    async getAllNotifications(body) {
        const { data: notis, totalCount } = await this.notificationService.listNotifications(body);
        const currentPage = body.page || 1;
        const limit = body.limit || 10;
        const totalPages = Math.ceil(totalCount / limit);
        return {
            status: 'success',
            message: 'Get all notification success!',
            pagination: {
                currentPage,
                limit,
                totalItems: totalCount,
                totalPages,
            },
            data: notis,
        };
    }
    async updateNotification(notiId, body) {
        const noti = await this.notificationService.updateNotification({
            notiId: notiId,
            isRead: body.isRead,
        });
        return {
            status: 'success',
            message: 'Update notification success!',
            data: noti,
        };
    }
    async bulkUpdateNotification(body) {
        const res = await this.notificationService.bulkUpdateNotification({
            isRead: body.isRead,
            userId: body.userId,
        });
        return {
            status: 'success',
            message: 'Update notification success!',
            data: res,
        };
    }
};
exports.NotificationController = NotificationController;
__decorate([
    (0, common_1.Post)(),
    __param(0, (0, common_1.Body)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object]),
    __metadata("design:returntype", Promise)
], NotificationController.prototype, "sendNotification", null);
__decorate([
    (0, common_1.Post)('/get-all'),
    __param(0, (0, common_1.Body)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object]),
    __metadata("design:returntype", Promise)
], NotificationController.prototype, "getAllNotifications", null);
__decorate([
    (0, common_1.Put)(':notiId'),
    __param(0, (0, common_1.Param)('notiId')),
    __param(1, (0, common_1.Body)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [String, Object]),
    __metadata("design:returntype", Promise)
], NotificationController.prototype, "updateNotification", null);
__decorate([
    (0, common_1.Post)('/update-all'),
    __param(0, (0, common_1.Body)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object]),
    __metadata("design:returntype", Promise)
], NotificationController.prototype, "bulkUpdateNotification", null);
exports.NotificationController = NotificationController = __decorate([
    (0, common_1.Controller)('/'),
    __metadata("design:paramtypes", [typeof (_a = typeof notification_service_1.NotificationService !== "undefined" && notification_service_1.NotificationService) === "function" ? _a : Object])
], NotificationController);


/***/ }),
/* 50 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.HealthController = void 0;
const common_1 = __webpack_require__(4);
let HealthController = class HealthController {
    async healthCheck() {
        return {
            status: 'ok',
            message: 'Notification service is healthy',
            timestamp: new Date().toISOString(),
        };
    }
};
exports.HealthController = HealthController;
__decorate([
    (0, common_1.Get)(),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", []),
    __metadata("design:returntype", Promise)
], HealthController.prototype, "healthCheck", null);
exports.HealthController = HealthController = __decorate([
    (0, common_1.Controller)('/health')
], HealthController);


/***/ }),
/* 51 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a, _b;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.MailService = void 0;
const core_1 = __webpack_require__(1);
const common_1 = __webpack_require__(4);
const mail_sender_interface_1 = __webpack_require__(52);
let MailService = class MailService {
    userClientService;
    mailSender;
    constructor(userClientService, mailSender) {
        this.userClientService = userClientService;
        this.mailSender = mailSender;
    }
    async sendVerifyOtp(input) {
        const { userId, otp } = input;
        if (!otp || !userId) {
            throw new Error('Invalid OTP || User Id');
        }
        const user = await this.userClientService.getUserById({ userId: userId });
        if (!user) {
            throw new Error('User not found!');
        }
        const data = {
            NAME: `${user.firstName} ${user.lastName}`,
            OTP: otp,
            EXPIRE_MINUTES: 5,
            YEAR: 2025,
        };
        await this.mailSender.sendMail({
            subject: 'test',
            to: user.email,
            template: 'verify-otp.hbs',
            data,
        });
    }
};
exports.MailService = MailService;
exports.MailService = MailService = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof core_1.UserClientService !== "undefined" && core_1.UserClientService) === "function" ? _a : Object, typeof (_b = typeof mail_sender_interface_1.IMailSender !== "undefined" && mail_sender_interface_1.IMailSender) === "function" ? _b : Object])
], MailService);


/***/ }),
/* 52 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.IMailSender = void 0;
class IMailSender {
}
exports.IMailSender = IMailSender;


/***/ }),
/* 53 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.MailSenderRepo = void 0;
const common_1 = __webpack_require__(4);
const config_1 = __webpack_require__(3);
const nodemailer = __webpack_require__(54);
const fs = __webpack_require__(55);
const handlebars = __webpack_require__(56);
const path_1 = __webpack_require__(18);
let MailSenderRepo = class MailSenderRepo {
    configService;
    transporter;
    constructor(configService) {
        this.configService = configService;
        this.transporter = nodemailer.createTransport({
            host: this.configService.get('MAIL_SMTP_HOST', 'smtp.mailtrap.io'),
            port: this.configService.get('MAIL_SMTP_PORT', 587),
            secure: false,
            auth: {
                user: this.configService.get('MAIL_SMTP_USER'),
                pass: this.configService.get('MAIL_SMTP_PASS'),
            },
        });
    }
    async sendMail(options) {
        let html = options.html;
        if (options.template && options.data) {
            const templatePath = (0, path_1.join)(__dirname, 'public', 'templates', `${options.template}`);
            const template = fs.readFileSync(templatePath, 'utf8');
            const compiled = handlebars.compile(template);
            html = compiled(options.data);
        }
        await this.transporter.sendMail({
            from: `"My App" <${this.configService.get('MAIL_SMTP_USER')}>`,
            to: options.to,
            subject: options.subject,
            text: options.text,
            html,
        });
    }
};
exports.MailSenderRepo = MailSenderRepo;
exports.MailSenderRepo = MailSenderRepo = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof config_1.ConfigService !== "undefined" && config_1.ConfigService) === "function" ? _a : Object])
], MailSenderRepo);


/***/ }),
/* 54 */
/***/ ((module) => {

module.exports = require("nodemailer");

/***/ }),
/* 55 */
/***/ ((module) => {

module.exports = require("fs");

/***/ }),
/* 56 */
/***/ ((module) => {

module.exports = require("handlebars");

/***/ })
/******/ 	]);
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
var __webpack_exports__ = {};
// This entry needs to be wrapped in an IIFE because it needs to be isolated against other modules in the chunk.
(() => {
var exports = __webpack_exports__;

Object.defineProperty(exports, "__esModule", ({ value: true }));
const core_1 = __webpack_require__(1);
const common_1 = __webpack_require__(4);
const core_2 = __webpack_require__(31);
const snakeCase_interceptor_1 = __webpack_require__(32);
const notification_module_1 = __webpack_require__(35);
async function bootstrap() {
    const app = await core_2.NestFactory.create(notification_module_1.NotificationModule);
    app.enableCors({
        origin: 'http://localhost:5173',
        methods: 'GET,HEAD,PUT,PATCH,POST,DELETE',
        credentials: true,
    });
    app.setGlobalPrefix('notification-service/api/v1');
    app.useGlobalInterceptors(new core_1.HttpAuthInterceptor(), new snakeCase_interceptor_1.TransformResponseInterceptor(), new core_1.GlobalHandleErrorInterceptor());
    await app.listen(5002);
    common_1.Logger.log(`🚀 HTTP server is running on http://localhost:5002/notification-service/api/v1`);
}
bootstrap();

})();

/******/ })()
;