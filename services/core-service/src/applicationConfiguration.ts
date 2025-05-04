import { plainToClassFromExist } from 'class-transformer';
import * as fs from 'fs';
import * as yaml from 'yaml';
import * as envsub from 'envsub';

export class ApplicationConfiguration {
  name: string;
  host: string = '172.0.0.1';
  tags: string[];
  user_token_trusted: boolean = false;
  rest: RestConfiguration;
  log: LogConfiguration;

  public constructor(init?: Partial<ApplicationConfiguration>) {
    if (init) {
      plainToClassFromExist(this, init);
    }
  }

  static async load(path: string): Promise<ApplicationConfiguration> {
    const configTemp = './config.temp.yml';
    await envsub({ templateFile: path, outputFile: configTemp });
    const yamlContent = fs.readFileSync(configTemp, 'utf8');

    const parsedConfig: unknown = yaml.parse(yamlContent);

    if (parsedConfig && typeof parsedConfig === 'object') {
      return new ApplicationConfiguration(
        parsedConfig as Partial<ApplicationConfiguration>,
      );
    } else {
      throw new Error('Failed to load a valid configuration');
    }
  }
}

class RestConfiguration {
  port: number;
  context: string;
}
export class LogConfiguration {
  hosts: string[];
  level: string = 'info';
}
