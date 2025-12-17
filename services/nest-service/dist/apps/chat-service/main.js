/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ([
/* 0 */,
/* 1 */
/***/ ((module) => {

module.exports = require("@nestjs/common");

/***/ }),
/* 2 */
/***/ ((module) => {

module.exports = require("@nestjs/core");

/***/ }),
/* 3 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.ChatServiceModule = void 0;
const common_1 = __webpack_require__(1);
const mongoose_1 = __webpack_require__(4);
const core_1 = __webpack_require__(5);
const chat_gateway_1 = __webpack_require__(28);
const chat_service_1 = __webpack_require__(30);
const chat_repo_1 = __webpack_require__(38);
const chat_schema_1 = __webpack_require__(40);
const auth_module_1 = __webpack_require__(41);
const ws_auth_adapter_1 = __webpack_require__(33);
let ChatServiceModule = class ChatServiceModule {
};
exports.ChatServiceModule = ChatServiceModule;
exports.ChatServiceModule = ChatServiceModule = __decorate([
    (0, common_1.Module)({
        imports: [
            core_1.CoreModule,
            auth_module_1.AuthModule,
            mongoose_1.MongooseModule.forFeature([
                { name: chat_schema_1.Message.name, schema: chat_schema_1.MessageSchema },
                { name: chat_schema_1.Room.name, schema: chat_schema_1.RoomSchema },
            ]),
        ],
        providers: [chat_gateway_1.ChatGateway, chat_service_1.ChatService, chat_repo_1.ChatRepository, ws_auth_adapter_1.WsAuthAdapter, { provide: 'IChatRepo', useClass: chat_repo_1.ChatRepository }],
    })
], ChatServiceModule);


/***/ }),
/* 4 */
/***/ ((module) => {

module.exports = require("@nestjs/mongoose");

/***/ }),
/* 5 */
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
__exportStar(__webpack_require__(6), exports);
__exportStar(__webpack_require__(8), exports);
__exportStar(__webpack_require__(9), exports);
__exportStar(__webpack_require__(19), exports);
__exportStar(__webpack_require__(21), exports);
__exportStar(__webpack_require__(23), exports);
__exportStar(__webpack_require__(26), exports);
__exportStar(__webpack_require__(12), exports);
__exportStar(__webpack_require__(17), exports);
__exportStar(__webpack_require__(18), exports);
__exportStar(__webpack_require__(16), exports);
__exportStar(__webpack_require__(27), exports);


/***/ }),
/* 6 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.CoreModule = void 0;
const config_1 = __webpack_require__(7);
const common_1 = __webpack_require__(1);
const log_service_1 = __webpack_require__(8);
const kafka_service_1 = __webpack_require__(9);
const mongoose_1 = __webpack_require__(4);
const user_client_service_1 = __webpack_require__(12);
const microservices_1 = __webpack_require__(13);
const path_1 = __webpack_require__(15);
const project_client_service_1 = __webpack_require__(16);
const sprint_client_service_1 = __webpack_require__(17);
const issue_client_service_1 = __webpack_require__(18);
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
                            url: configService.get('MAIN_SERVICE') || '0.0.0.0:5001',
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
                            url: configService.get('MAIN_SERVICE'),
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
                            url: configService.get('MAIN_SERVICE'),
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
                            url: configService.get('MAIN_SERVICE'),
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
        providers: [log_service_1.LogService, kafka_service_1.KafkaService, user_client_service_1.UserClientService, project_client_service_1.ProjectClientService, sprint_client_service_1.SprintClientService, issue_client_service_1.IssueClientService],
        exports: [log_service_1.LogService, kafka_service_1.KafkaService, user_client_service_1.UserClientService, project_client_service_1.ProjectClientService, sprint_client_service_1.SprintClientService, issue_client_service_1.IssueClientService],
    })
], CoreModule);


/***/ }),
/* 7 */
/***/ ((module) => {

module.exports = require("@nestjs/config");

/***/ }),
/* 8 */
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
const common_1 = __webpack_require__(1);
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
exports.KafkaService = exports.KafkaActionType = void 0;
const common_1 = __webpack_require__(1);
const kafkajs_1 = __webpack_require__(10);
const uuid_1 = __webpack_require__(11);
const log_service_1 = __webpack_require__(8);
const config_1 = __webpack_require__(7);
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
    constructor(logService, configService) {
        this.logService = logService;
        this.configService = configService;
        const clientId = this.configService.get('KAFKA_CLIENT_ID', 'taskflow-client');
        const brokers = this.configService.get('KAFKA_BROKERS', 'kafka:9092').split(',');
        this.kafka = new kafkajs_1.Kafka({
            clientId,
            brokers,
        });
        this.producer = this.kafka.producer({
            createPartitioner: kafkajs_1.Partitioners.LegacyPartitioner,
        });
    }
    async onModuleInit() {
        await this.producer.connect();
        this.logService.log('Kafka Producer connected');
    }
    async onModuleDestroy() {
        for (const consumer of this.consumers) {
            await consumer.disconnect();
        }
        await this.producer.disconnect();
        this.logService.log('Kafka disconnected');
    }
    async publish(topic, message, config = {}) {
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
/* 10 */
/***/ ((module) => {

module.exports = require("kafkajs");

/***/ }),
/* 11 */
/***/ ((module) => {

module.exports = require("uuid");

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
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var _a;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.UserClientService = void 0;
const common_1 = __webpack_require__(1);
const microservices_1 = __webpack_require__(13);
const rxjs_1 = __webpack_require__(14);
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
/* 13 */
/***/ ((module) => {

module.exports = require("@nestjs/microservices");

/***/ }),
/* 14 */
/***/ ((module) => {

module.exports = require("rxjs");

/***/ }),
/* 15 */
/***/ ((module) => {

module.exports = require("path");

/***/ }),
/* 16 */
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
const common_1 = __webpack_require__(1);
const microservices_1 = __webpack_require__(13);
const rxjs_1 = __webpack_require__(14);
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
/* 17 */
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
const common_1 = __webpack_require__(1);
const microservices_1 = __webpack_require__(13);
const rxjs_1 = __webpack_require__(14);
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
/* 18 */
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
const common_1 = __webpack_require__(1);
const microservices_1 = __webpack_require__(13);
const rxjs_1 = __webpack_require__(14);
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
        const res = await (0, rxjs_1.firstValueFrom)(this.issueGrpcService.listIssues({ page: params.page, limit: params.limit, assigneeIds: [], columnIds: [], issueIds: params.issueIds, sprintIds: [], projectId: '' }));
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
var _a, _b;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.CassandraService = void 0;
const common_1 = __webpack_require__(1);
const config_1 = __webpack_require__(7);
const cassandra_driver_1 = __webpack_require__(20);
const log_service_1 = __webpack_require__(8);
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
/* 20 */
/***/ ((module) => {

module.exports = require("cassandra-driver");

/***/ }),
/* 21 */
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
const common_1 = __webpack_require__(1);
const rxjs_1 = __webpack_require__(14);
const microservices_1 = __webpack_require__(13);
const grpc_js_1 = __webpack_require__(22);
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
/* 22 */
/***/ ((module) => {

module.exports = require("@grpc/grpc-js");

/***/ }),
/* 23 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.GrpcAuthInterceptor = void 0;
const common_1 = __webpack_require__(1);
const microservices_1 = __webpack_require__(13);
const constants_1 = __webpack_require__(24);
const jwt_decode_1 = __webpack_require__(25);
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
/* 24 */
/***/ ((module) => {

module.exports = require("@grpc/grpc-js/build/src/constants");

/***/ }),
/* 25 */
/***/ ((module) => {

module.exports = require("jwt-decode");

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
exports.HttpAuthInterceptor = void 0;
const common_1 = __webpack_require__(1);
const jwt_decode_1 = __webpack_require__(25);
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
/* 27 */
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.UserMetadata = void 0;
const common_1 = __webpack_require__(1);
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
/* 28 */
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
var ChatGateway_1;
var _a, _b, _c, _d;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.ChatGateway = void 0;
const websockets_1 = __webpack_require__(29);
const common_1 = __webpack_require__(1);
const chat_service_1 = __webpack_require__(30);
const core_1 = __webpack_require__(5);
const ws_auth_adapter_1 = __webpack_require__(33);
const mapper_1 = __webpack_require__(36);
const ws_1 = __webpack_require__(37);
let ChatGateway = ChatGateway_1 = class ChatGateway {
    chatService;
    userClientService;
    wsAuthAdapter;
    server;
    connectedUsers = new Map();
    logger = new common_1.Logger(ChatGateway_1.name);
    constructor(chatService, userClientService, wsAuthAdapter) {
        this.chatService = chatService;
        this.userClientService = userClientService;
        this.wsAuthAdapter = wsAuthAdapter;
    }
    async handleConnection(ws, request) {
        this.logger.log(`New connection attempt: ${request.headers['sec-websocket-key']}`);
        try {
            const cookieHeader = request.headers.cookie;
            if (!cookieHeader) {
                this.logger.error('No cookies provided');
                ws.send(JSON.stringify({
                    event: 'error',
                    data: {
                        code: 'AUTH_NO_COOKIES',
                        message: 'No cookies provided',
                        details: {
                            requiredHeader: 'cookie',
                        },
                    },
                }));
                ws.close();
                return;
            }
            const metadata = this.wsAuthAdapter.createMetadataFromCookie(cookieHeader);
            const user = await this.userClientService.getUserById({ metadata });
            if (!user) {
                this.logger.error('Authentication required');
                ws.send(JSON.stringify({
                    event: 'error',
                    data: {
                        code: 'AUTH_REQUIRED',
                        message: 'Authentication required',
                        details: {
                            reason: 'User not found',
                        },
                    },
                }));
                ws.close();
                return;
            }
            ws.userId = user.id;
            ws.roomIds = new Set();
            if (!this.connectedUsers.has(user.id)) {
                this.connectedUsers.set(user.id, new Set());
            }
            this.connectedUsers.get(user.id)?.add(ws);
            ws.on('message', (data) => {
                try {
                    const message = JSON.parse(data.toString());
                    this.handleMessage(ws, message);
                }
                catch {
                    this.logger.error('Invalid message format');
                    ws.send(JSON.stringify({ event: 'error', data: { message: 'Invalid message format' } }));
                }
            });
            ws.on('close', () => this.handleDisconnect(ws));
            ws.send(JSON.stringify({
                event: 'connection_ack',
                data: {
                    status: 'connected',
                    user: {
                        id: user.id,
                        firstName: user.firstName,
                        lastName: user.lastName,
                        email: user.email,
                        role: user.role,
                        connectionId: request.headers['sec-websocket-key'],
                    },
                },
            }));
            this.logger.log(`User ${user.id} connected`);
        }
        catch (error) {
            this.logger.error(`Connection error: ${error.message}`);
            ws.send(JSON.stringify({ event: 'error', data: { message: 'Connection failed' } }));
            ws.close();
        }
    }
    handleDisconnect(ws) {
        const userId = ws.userId;
        if (!userId)
            return;
        this.logger.log(`User ${userId} disconnected`);
        const userSockets = this.connectedUsers.get(userId);
        if (userSockets) {
            userSockets.delete(ws);
            if (userSockets.size === 0) {
                this.connectedUsers.delete(userId);
            }
        }
    }
    async handleMessage(ws, message) {
        const userId = ws.userId;
        if (!userId)
            return;
        try {
            switch (message.event) {
                case 'joinRoom':
                    await this.handleJoinRoom(ws, { roomId: message.data });
                    break;
                case 'sendMessage':
                    await this.handleSendMessage(ws, message.data);
                    break;
                case 'getRooms':
                    await this.handleGetRooms(ws);
                    break;
                default:
                    this.logger.warn(`Unknown event: ${message.event}`);
                    ws.send(JSON.stringify({ event: 'error', data: { message: 'Unknown event' } }));
            }
        }
        catch (error) {
            this.logger.error(`Message processing error: ${error.message}`);
            ws.send(JSON.stringify({ event: 'error', data: { message: 'Failed to process message' } }));
        }
    }
    async handleJoinRoom(ws, data) {
        try {
            const userId = ws.userId;
            const existingRoom = await this.chatService.getRoomById(data.roomId);
            if (!existingRoom || !existingRoom.members.includes(userId)) {
                this.logger.error(`User ${userId} is not a member of room ${data.roomId}`);
                ws.send(JSON.stringify({
                    event: 'error',
                    data: {
                        code: 'ROOM_ACCESS_DENIED',
                        message: 'Not a member of this room',
                        details: {
                            roomId: data.roomId,
                            userId,
                        },
                    },
                }));
                return;
            }
            ws.roomIds.add(data.roomId);
            const messages = await this.chatService.getMessagesByRoomId(data.roomId, 50);
            this.logger.log(`Found ${messages.length} messages for room ${data.roomId}`);
            if (existingRoom.lastMessage) {
                this.logger.log('LastMessage id:', existingRoom.lastMessage.id);
                this.logger.log('LastMessage createdAt:', existingRoom.lastMessage.createdAt);
                this.logger.log('Is createdAt a Date?', existingRoom.lastMessage.createdAt instanceof Date);
                this.logger.log('Is createdAt valid?', !isNaN(existingRoom.lastMessage.createdAt.getTime()));
            }
            const roomResponse = mapper_1.ChatMapper.toRoomResponse(existingRoom);
            const messageResponses = messages.map((msg) => ({
                id: msg.id,
                roomId: msg.roomId,
                senderId: msg.senderId,
                content: msg.content,
                type: msg.type,
                replyToId: msg.replyToId,
                createdAt: msg.createdAt.toISOString(),
                updatedAt: msg.updatedAt?.toISOString(),
            }));
            ws.send(JSON.stringify({
                event: 'roomJoined',
                data: {
                    room: roomResponse,
                    messages: messageResponses,
                    joinedAt: new Date().toISOString(),
                },
            }));
            this.broadcastToRoom(data.roomId, 'userJoined', {
                roomId: data.roomId,
                userId,
                timestamp: new Date().toISOString(),
                userCount: existingRoom.members.length,
            }, [userId]);
            this.logger.log(`User ${userId} joined room ${data.roomId}`);
        }
        catch (error) {
            this.logger.error(`Failed to join room: ${error.message}`);
            ws.send(JSON.stringify({
                event: 'error',
                data: {
                    code: 'ROOM_JOIN_FAILED',
                    message: 'Failed to join room',
                    details: {
                        roomId: data.roomId,
                        error: error.message,
                    },
                },
            }));
        }
    }
    async handleSendMessage(ws, data) {
        try {
            const userId = ws.userId;
            const message = await this.chatService.createMessage({
                roomId: data.roomId,
                senderId: userId,
                content: data.content,
                type: this.chatService.validateMessageType(data.type),
                replyToId: data.replyToId,
            });
            const messageResponse = mapper_1.ChatMapper.toMessageResponse(message);
            ws.send(JSON.stringify({
                event: 'messageSent',
                data: {
                    message: messageResponse,
                    status: 'delivered',
                    timestamp: new Date().toISOString(),
                },
            }));
            this.broadcastToRoom(data.roomId, 'messageReceived', {
                ...messageResponse,
                timestamp: new Date().toISOString(),
            });
            this.logger.log(`Message sent in room ${data.roomId} by user ${userId}`);
        }
        catch (error) {
            this.logger.error(`Failed to send message: ${error.message}`);
            ws.send(JSON.stringify({
                event: 'error',
                data: {
                    code: 'MESSAGE_SEND_FAILED',
                    message: 'Failed to send message',
                    details: {
                        roomId: data.roomId,
                        error: error.message,
                    },
                },
            }));
        }
    }
    async handleGetRooms(ws) {
        try {
            const userId = ws.userId;
            const rooms = await this.chatService.getRoomsByUserId(userId);
            this.logger.log('Rooms fetched for getRooms:', JSON.stringify(rooms, null, 2));
            const roomDomains = rooms.map((doc) => mapper_1.ChatMapper.toRoomDomain(doc));
            const roomResponses = roomDomains.map((room) => mapper_1.ChatMapper.toRoomResponse(room));
            ws.send(JSON.stringify({
                event: 'roomsList',
                data: {
                    rooms: roomResponses,
                    timestamp: new Date().toISOString(),
                    totalCount: roomResponses.length,
                    userId,
                },
            }));
        }
        catch (error) {
            this.logger.error(`Failed to fetch rooms: ${error.message}`);
            ws.send(JSON.stringify({
                event: 'error',
                data: {
                    code: 'ROOMS_FETCH_FAILED',
                    message: 'Failed to fetch rooms',
                    details: {
                        userId: ws.userId,
                        error: error.message,
                    },
                },
            }));
        }
    }
    broadcastToRoom(roomId, event, data, excludeUsers = []) {
        const message = JSON.stringify({ event, data });
        this.connectedUsers.forEach((sockets, userId) => {
            if (!excludeUsers.includes(userId)) {
                sockets.forEach((ws) => {
                    if (ws.roomIds.has(roomId)) {
                        try {
                            ws.send(message);
                        }
                        catch (error) {
                            this.logger.error(`Broadcast error to user ${userId}: ${error.message}`);
                        }
                    }
                });
            }
        });
    }
};
exports.ChatGateway = ChatGateway;
__decorate([
    (0, websockets_1.WebSocketServer)(),
    __metadata("design:type", typeof (_d = typeof ws_1.WebSocket !== "undefined" && ws_1.WebSocket.Server) === "function" ? _d : Object)
], ChatGateway.prototype, "server", void 0);
exports.ChatGateway = ChatGateway = ChatGateway_1 = __decorate([
    (0, common_1.Injectable)(),
    (0, websockets_1.WebSocketGateway)(5003, { cors: true }),
    __metadata("design:paramtypes", [typeof (_a = typeof chat_service_1.ChatService !== "undefined" && chat_service_1.ChatService) === "function" ? _a : Object, typeof (_b = typeof core_1.UserClientService !== "undefined" && core_1.UserClientService) === "function" ? _b : Object, typeof (_c = typeof ws_auth_adapter_1.WsAuthAdapter !== "undefined" && ws_auth_adapter_1.WsAuthAdapter) === "function" ? _c : Object])
], ChatGateway);


/***/ }),
/* 29 */
/***/ ((module) => {

module.exports = require("@nestjs/websockets");

/***/ }),
/* 30 */
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
exports.ChatService = void 0;
const common_1 = __webpack_require__(1);
const microservices_1 = __webpack_require__(13);
const grpc_js_1 = __webpack_require__(22);
const chat_repo_interface_1 = __webpack_require__(31);
const chat_1 = __webpack_require__(32);
let ChatService = class ChatService {
    chatRepo;
    constructor(chatRepo) {
        this.chatRepo = chatRepo;
    }
    async createMessage(params) {
        const message = await this.chatRepo.createMessage({
            ...params,
            createdAt: new Date(),
        });
        await this.chatRepo.updateLastMessage(params.roomId, message);
        return message;
    }
    async getMessagesByRoomId(roomId, limit = 50, before) {
        return this.chatRepo.getMessagesByRoomId(roomId, limit, before);
    }
    async createRoom(params) {
        if (params.type === 'DIRECT' && params.members.length !== 2) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INVALID_ARGUMENT,
                message: 'Direct chat must have exactly 2 members',
            });
        }
        if (params.type === 'GROUP' && params.members.length < 2) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INVALID_ARGUMENT,
                message: 'Group chat must have at least 2 members',
            });
        }
        return this.chatRepo.createRoom({
            ...params,
            createdAt: new Date(),
        });
    }
    async getRoomById(id) {
        const room = await this.chatRepo.getRoomById(id);
        if (!room) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.NOT_FOUND,
                message: 'Room not found',
            });
        }
        return room;
    }
    async getRoomsByUserId(userId) {
        return this.chatRepo.getRoomsByUserId(userId);
    }
    async addMemberToRoom(roomId, userId) {
        const room = await this.getRoomById(roomId);
        if (room.type === 'DIRECT') {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INVALID_ARGUMENT,
                message: 'Cannot add members to direct chat',
            });
        }
        if (room.members.includes(userId)) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.ALREADY_EXISTS,
                message: 'User is already a member of this room',
            });
        }
        return this.chatRepo.addMemberToRoom(roomId, userId);
    }
    async removeMemberFromRoom(roomId, userId) {
        const room = await this.getRoomById(roomId);
        if (room.type === 'DIRECT') {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INVALID_ARGUMENT,
                message: 'Cannot remove members from direct chat',
            });
        }
        if (!room.members.includes(userId)) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.NOT_FOUND,
                message: 'User is not a member of this room',
            });
        }
        if (room.members.length <= 2) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INVALID_ARGUMENT,
                message: 'Cannot remove member from room with only 2 members',
            });
        }
        return this.chatRepo.removeMemberFromRoom(roomId, userId);
    }
    validateMessageType(type) {
        if (!type || !Object.values(chat_1.MessageType).includes(type)) {
            throw new microservices_1.RpcException({
                code: grpc_js_1.status.INVALID_ARGUMENT,
                message: 'Invalid message type',
            });
        }
        return type;
    }
};
exports.ChatService = ChatService;
exports.ChatService = ChatService = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, common_1.Inject)('IChatRepo')),
    __metadata("design:paramtypes", [typeof (_a = typeof chat_repo_interface_1.IChatRepo !== "undefined" && chat_repo_interface_1.IChatRepo) === "function" ? _a : Object])
], ChatService);


/***/ }),
/* 31 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.IChatRepo = void 0;
class IChatRepo {
}
exports.IChatRepo = IChatRepo;


/***/ }),
/* 32 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.MessageType = void 0;
var MessageType;
(function (MessageType) {
    MessageType["TEXT"] = "TEXT";
    MessageType["IMAGE"] = "IMAGE";
    MessageType["FILE"] = "FILE";
    MessageType["SYSTEM"] = "SYSTEM";
})(MessageType || (exports.MessageType = MessageType = {}));


/***/ }),
/* 33 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.WsAuthAdapter = void 0;
const grpc_js_1 = __webpack_require__(22);
const common_1 = __webpack_require__(1);
const cookie_1 = __webpack_require__(34);
const jwt = __webpack_require__(35);
let WsAuthAdapter = class WsAuthAdapter {
    createMetadataFromCookie(cookieHeader) {
        const metadata = new grpc_js_1.Metadata();
        if (!cookieHeader) {
            return metadata;
        }
        try {
            const cookies = (0, cookie_1.parse)(cookieHeader);
            const token = cookies.token;
            if (!token) {
                return metadata;
            }
            const decoded = jwt.decode(token);
            if (decoded?.userId) {
                metadata.set('userId', decoded.userId);
            }
            if (decoded?.role) {
                metadata.set('userRole', decoded.role);
            }
        }
        catch (err) {
            console.error('Error parsing auth cookie:', err);
        }
        return metadata;
    }
};
exports.WsAuthAdapter = WsAuthAdapter;
exports.WsAuthAdapter = WsAuthAdapter = __decorate([
    (0, common_1.Injectable)()
], WsAuthAdapter);


/***/ }),
/* 34 */
/***/ ((module) => {

module.exports = require("cookie");

/***/ }),
/* 35 */
/***/ ((module) => {

module.exports = require("jsonwebtoken");

/***/ }),
/* 36 */
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.ChatMapper = void 0;
class ChatMapper {
    static toMessageDomain(doc) {
        return {
            id: doc._id.toString(),
            roomId: doc.roomId,
            senderId: doc.senderId,
            content: doc.content,
            type: doc.type,
            replyToId: doc.replyToId,
            createdAt: doc.createdAt,
            updatedAt: doc.updatedAt,
        };
    }
    static toRoomDomain(doc) {
        let lastMessage = undefined;
        if (doc.lastMessage) {
            const messageDoc = doc.lastMessage;
            if (messageDoc._id) {
                lastMessage = {
                    id: messageDoc._id.toString(),
                    roomId: messageDoc.roomId,
                    senderId: messageDoc.senderId,
                    content: messageDoc.content,
                    type: messageDoc.type,
                    replyToId: messageDoc.replyToId ?? undefined,
                    createdAt: messageDoc.createdAt instanceof Date ? messageDoc.createdAt : new Date(messageDoc.createdAt),
                    updatedAt: messageDoc.updatedAt ? (messageDoc.updatedAt instanceof Date ? messageDoc.updatedAt : new Date(messageDoc.updatedAt)) : undefined,
                };
            }
        }
        return {
            id: doc._id.toString(),
            name: doc.name,
            type: doc.type,
            members: doc.members,
            lastMessage,
            createdAt: doc.createdAt,
            updatedAt: doc.updatedAt,
        };
    }
    static toMessageResponse(message) {
        const id = message.id || message._id?.toString();
        if (!id) {
            console.warn('No ID found for message, generating temporary one');
            return {
                id: `temp-${Date.now()}`,
                roomId: message.roomId,
                senderId: message.senderId,
                content: '[Unknown]',
                type: message.type || 'TEXT',
                replyToId: message.replyToId,
                createdAt: new Date().toISOString(),
            };
        }
        if (!message.createdAt) {
            console.warn('Missing createdAt in message', id);
            message.createdAt = new Date();
        }
        else if (!(message.createdAt instanceof Date)) {
            const d = new Date(message.createdAt);
            message.createdAt = isNaN(d.getTime()) ? new Date() : d;
        }
        return {
            id,
            roomId: message.roomId,
            senderId: message.senderId,
            content: message.content,
            type: message.type,
            replyToId: message.replyToId,
            createdAt: message.createdAt.toISOString(),
        };
    }
    static toRoomResponse(room) {
        if (!room.id) {
            throw new Error('Room must have an id');
        }
        return {
            id: room.id,
            name: room.name,
            type: room.type,
            members: room.members,
            lastMessage: room.lastMessage ? this.toMessageResponse(room.lastMessage) : undefined,
            createdAt: (room.createdAt || new Date()).toISOString(),
        };
    }
}
exports.ChatMapper = ChatMapper;


/***/ }),
/* 37 */
/***/ ((module) => {

module.exports = require("ws");

/***/ }),
/* 38 */
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
exports.ChatRepository = void 0;
const common_1 = __webpack_require__(1);
const mongoose_1 = __webpack_require__(4);
const mongoose_2 = __webpack_require__(39);
let ChatRepository = class ChatRepository {
    messageModel;
    roomModel;
    logger;
    constructor(messageModel, roomModel) {
        this.messageModel = messageModel;
        this.roomModel = roomModel;
    }
    async createMessage(data) {
        const message = new this.messageModel({
            ...data,
            createdAt: new Date(),
        });
        return message.save();
    }
    async createRoom(data) {
        const room = new this.roomModel({
            ...data,
            createdAt: new Date(),
        });
        return room.save();
    }
    async getMessagesByRoomId(roomId, limit, before) {
        console.log('Querying messages for roomId:', roomId);
        const query = this.messageModel.find({ roomId });
        if (before) {
            query.where('createdAt').lt(before.getTime());
        }
        if (limit) {
            query.limit(limit);
        }
        const results = await query.sort({ createdAt: -1 }).lean().exec();
        const candidates = results.map((doc) => {
            const id = doc._id?.toString();
            const createdAt = doc.createdAt ? new Date(doc.createdAt) : null;
            if (!id || !createdAt || isNaN(createdAt.getTime())) {
                console.warn('Invalid message skipped:', { _id: doc._id, createdAt: doc.createdAt });
                return null;
            }
            const message = {
                id,
                roomId: doc.roomId,
                senderId: doc.senderId,
                content: doc.content,
                type: doc.type,
                replyToId: doc.replyToId ?? undefined,
                createdAt,
                updatedAt: doc.updatedAt ? new Date(doc.updatedAt) : undefined,
            };
            return message;
        });
        const messages = candidates.filter((msg) => msg !== null);
        console.log('Messages found:', messages.length, messages.map((m) => m.id));
        return messages;
    }
    async getRoomsByUserId(userId) {
        return this.roomModel.find({ members: userId }).sort({ updatedAt: -1 }).exec();
    }
    async addMemberToRoom(roomId, userId) {
        return this.roomModel.findOneAndUpdate({ _id: roomId }, { $addToSet: { members: userId }, updatedAt: new Date() }, { new: true }).exec();
    }
    async removeMemberFromRoom(roomId, userId) {
        return this.roomModel.findOneAndUpdate({ _id: roomId }, { $pull: { members: userId }, updatedAt: new Date() }, { new: true }).exec();
    }
    async getRoomById(roomId) {
        return this.roomModel.findById(roomId).exec();
    }
    async updateLastMessage(roomId, message) {
        return this.roomModel.findByIdAndUpdate(roomId, { lastMessage: message, updatedAt: new Date() }, { new: true }).exec();
    }
};
exports.ChatRepository = ChatRepository;
exports.ChatRepository = ChatRepository = __decorate([
    (0, common_1.Injectable)(),
    __param(0, (0, mongoose_1.InjectModel)('Message')),
    __param(1, (0, mongoose_1.InjectModel)('Room')),
    __metadata("design:paramtypes", [typeof (_a = typeof mongoose_2.Model !== "undefined" && mongoose_2.Model) === "function" ? _a : Object, typeof (_b = typeof mongoose_2.Model !== "undefined" && mongoose_2.Model) === "function" ? _b : Object])
], ChatRepository);


/***/ }),
/* 39 */
/***/ ((module) => {

module.exports = require("mongoose");

/***/ }),
/* 40 */
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
var _a, _b, _c, _d, _e;
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.RoomSchema = exports.MessageSchema = exports.Room = exports.Message = void 0;
const mongoose_1 = __webpack_require__(4);
const chat_1 = __webpack_require__(32);
let Message = class Message {
    roomId;
    senderId;
    content;
    type;
    replyToId;
    createdAt;
    updatedAt;
};
exports.Message = Message;
__decorate([
    (0, mongoose_1.Prop)({ required: true }),
    __metadata("design:type", String)
], Message.prototype, "roomId", void 0);
__decorate([
    (0, mongoose_1.Prop)({ required: true }),
    __metadata("design:type", String)
], Message.prototype, "senderId", void 0);
__decorate([
    (0, mongoose_1.Prop)({ required: true }),
    __metadata("design:type", String)
], Message.prototype, "content", void 0);
__decorate([
    (0, mongoose_1.Prop)({ required: true, enum: chat_1.MessageType }),
    __metadata("design:type", String)
], Message.prototype, "type", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", String)
], Message.prototype, "replyToId", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", typeof (_a = typeof Date !== "undefined" && Date) === "function" ? _a : Object)
], Message.prototype, "createdAt", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", typeof (_b = typeof Date !== "undefined" && Date) === "function" ? _b : Object)
], Message.prototype, "updatedAt", void 0);
exports.Message = Message = __decorate([
    (0, mongoose_1.Schema)({ timestamps: true, collection: 'messages' })
], Message);
let Room = class Room {
    name;
    type;
    members;
    lastMessage;
    createdAt;
    updatedAt;
};
exports.Room = Room;
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", String)
], Room.prototype, "name", void 0);
__decorate([
    (0, mongoose_1.Prop)({ required: true, enum: ['DIRECT', 'GROUP'] }),
    __metadata("design:type", String)
], Room.prototype, "type", void 0);
__decorate([
    (0, mongoose_1.Prop)({ type: [String], required: true }),
    __metadata("design:type", Array)
], Room.prototype, "members", void 0);
__decorate([
    (0, mongoose_1.Prop)({ type: Object }),
    __metadata("design:type", typeof (_c = typeof Record !== "undefined" && Record) === "function" ? _c : Object)
], Room.prototype, "lastMessage", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", typeof (_d = typeof Date !== "undefined" && Date) === "function" ? _d : Object)
], Room.prototype, "createdAt", void 0);
__decorate([
    (0, mongoose_1.Prop)(),
    __metadata("design:type", typeof (_e = typeof Date !== "undefined" && Date) === "function" ? _e : Object)
], Room.prototype, "updatedAt", void 0);
exports.Room = Room = __decorate([
    (0, mongoose_1.Schema)({ timestamps: true, collection: 'rooms' })
], Room);
exports.MessageSchema = mongoose_1.SchemaFactory.createForClass(Message);
exports.RoomSchema = mongoose_1.SchemaFactory.createForClass(Room);


/***/ }),
/* 41 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.AuthModule = void 0;
const common_1 = __webpack_require__(1);
const jwt_1 = __webpack_require__(42);
const config_1 = __webpack_require__(7);
const ws_auth_guard_1 = __webpack_require__(43);
let AuthModule = class AuthModule {
};
exports.AuthModule = AuthModule;
exports.AuthModule = AuthModule = __decorate([
    (0, common_1.Module)({
        imports: [
            jwt_1.JwtModule.registerAsync({
                imports: [config_1.ConfigModule],
                useFactory: (configService) => ({
                    secret: configService.getOrThrow('JWT_SECRET'),
                    signOptions: {
                        expiresIn: configService.get('JWT_EXPIRES_IN', '24h'),
                    },
                }),
                inject: [config_1.ConfigService],
            }),
        ],
        providers: [ws_auth_guard_1.WsAuthGuard],
        exports: [ws_auth_guard_1.WsAuthGuard, jwt_1.JwtModule],
    })
], AuthModule);


/***/ }),
/* 42 */
/***/ ((module) => {

module.exports = require("@nestjs/jwt");

/***/ }),
/* 43 */
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
exports.WsAuthGuard = void 0;
const common_1 = __webpack_require__(1);
const websockets_1 = __webpack_require__(29);
const jwt_1 = __webpack_require__(42);
let WsAuthGuard = class WsAuthGuard {
    jwtService;
    constructor(jwtService) {
        this.jwtService = jwtService;
    }
    async canActivate(context) {
        try {
            const client = context.switchToWs().getClient();
            const cookies = client.handshake?.headers?.cookie;
            if (!cookies) {
                throw new websockets_1.WsException('No cookies found');
            }
            const token = this.parseCookies(cookies)['token'];
            if (!token) {
                throw new websockets_1.WsException('Authentication token not found');
            }
            const payload = await this.jwtService.verifyAsync(token).catch(() => null);
            if (!payload?.sub) {
                throw new websockets_1.WsException('Invalid token payload');
            }
            client.data = {
                userId: payload.sub,
                ...payload,
            };
            return true;
        }
        catch (error) {
            if (error instanceof websockets_1.WsException) {
                throw error;
            }
            throw new websockets_1.WsException('Invalid authentication token');
        }
    }
    parseCookies(cookieHeader) {
        const cookies = {};
        cookieHeader.split(';').forEach((cookie) => {
            const parts = cookie.split('=');
            const name = parts[0].trim();
            const value = parts[1]?.trim();
            if (name && value) {
                cookies[name] = value;
            }
        });
        return cookies;
    }
};
exports.WsAuthGuard = WsAuthGuard;
exports.WsAuthGuard = WsAuthGuard = __decorate([
    (0, common_1.Injectable)(),
    __metadata("design:paramtypes", [typeof (_a = typeof jwt_1.JwtService !== "undefined" && jwt_1.JwtService) === "function" ? _a : Object])
], WsAuthGuard);


/***/ }),
/* 44 */
/***/ ((module) => {

module.exports = require("@nestjs/platform-ws");

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
const common_1 = __webpack_require__(1);
const core_1 = __webpack_require__(2);
const chat_service_module_1 = __webpack_require__(3);
const platform_ws_1 = __webpack_require__(44);
async function bootstrap() {
    try {
        const start = Date.now();
        common_1.Logger.log(`[BOOT] Starting bootstrap at ${new Date().toISOString()}`);
        const beforeCreate = Date.now();
        common_1.Logger.log(`[BOOT] Before NestFactory.create: ${beforeCreate - start}ms since start`);
        const app = await core_1.NestFactory.create(chat_service_module_1.ChatServiceModule, { cors: true });
        common_1.Logger.log(`[BOOT] After NestFactory.create: ${Date.now() - start}ms since start`);
        app.useWebSocketAdapter(new platform_ws_1.WsAdapter(app));
        const beforeInit = Date.now();
        common_1.Logger.log(`[BOOT] Before app.init: ${beforeInit - start}ms since start`);
        await app.init();
        common_1.Logger.log(`[BOOT] After app.init: ${Date.now() - start}ms since start`);
        common_1.Logger.log(`🚀 WebSocket server running on ws://localhost:5003`);
    }
    catch (error) {
        common_1.Logger.error(`Failed to start WebSocket server: ${error.message}`);
        process.exit(1);
    }
}
bootstrap();

})();

/******/ })()
;