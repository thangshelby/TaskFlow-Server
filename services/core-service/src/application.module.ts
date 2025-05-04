import { isEmpty, reduce } from 'lodash';
import { ApplicationService } from 'src/application.service';
import { ApplicationConfiguration } from 'src/applicationConfiguration';
import { LogService } from 'src/logger/log.service';
import { ClassType, Module } from 'src/module';
import { Container } from 'typedi';

export class Application {
  private DEFAULT_SERVICES: any[] = [];

  protected Services: ApplicationService[];
  protected Rests: ClassType[];
  constructor(
    protected cfg: ApplicationConfiguration | string,
    Rests: ClassType[] | Module,
    Services: any[] = [],
  ) {
    if (Array.isArray(Rests)) {
      this.Services = [...this.DEFAULT_SERVICES, ...Services];
      this.Rests = Rests;
    } else {
      const m = this.extractModule({ modules: [Rests] });
      this.Services = [...this.DEFAULT_SERVICES, ...m.services];
      this.Rests = m.rests;
    }
  }

  extractModule(module: Module): { rests: ClassType[]; services: ClassType[] } {
    if (isEmpty(module.modules)) {
      return { rests: module.rests || [], services: module.services || [] };
    }

    return reduce(
      module.modules,
      (result: { rests: ClassType[]; services: ClassType[] }, m: Module) => {
        const data = this.extractModule(m);
        result.rests = result.rests.concat(data.rests);
        result.services = result.services.concat(data.services);
        return result;
      },
      { rests: module.rests || [], services: module.services || [] },
    );
  }

  getCfg(): ApplicationConfiguration {
    return this.cfg as ApplicationConfiguration;
  }

  async start(): Promise<void> {
    this.cfg =
      typeof this.cfg === 'string'
        ? await ApplicationConfiguration.load(this.cfg)
        : new ApplicationConfiguration(this.cfg);

    const logService = await new LogService(this.cfg.log).start();
    logService.info('[Application] Application is starting...');

    Container.set('rests', this.Rests);
    Container.set(LogService, logService);
    Container.set(Application, this);
    Container.set(ApplicationConfiguration, this.cfg);

    try {
      logService.info(`[Application] Application services are starting...`);
      let Service: any;
      for (Service of this.Services) {
        if (
          !Service.isEnabled ||
          (Service.isEnabled && Service.isEnabled(this.cfg))
        )
          await (Container.get(Service) as any).start();
      }
      logService.info(`[Application] Application services are started`);
      logService.info(`[Application] Application is started`);

      // process.on("uncaughtException", function (err) {
      //     logService.error("uncaught exception: %o", err);
      // });

      // Init SIGINT hook
      process.on('SIGINT', async () => {
        await this.stop();
        process.exit(0);
      });
    } catch (ex) {
      logService.error(ex.stack || ex.message);
    }
  }
  async stop() {
    const logService = Container.get(LogService);
    try {
      logService.info(`[Application] Application is stopping...`);
      logService.info(`[Application] Application services is stopping...`);
      let Service: any;
      for (Service of this.Services) {
        if (
          !Service.isEnabled ||
          (Service.isEnabled && Service.isEnabled(this.cfg))
        )
          await (Container.get(Service) as ApplicationService).stop();
      }
      logService.info(`[Application] Application services is stopped`);
      logService.info(`[Application] Application is stopped`);
    } catch (ex) {
      logService.error(ex);
    }
  }
}
