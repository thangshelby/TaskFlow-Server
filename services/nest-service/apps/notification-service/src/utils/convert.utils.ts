const toSnakeCase = (str: string): string => {
  return str.replace(/([A-Z])/g, '_$1').toLowerCase();
};

const convertToSnakeCase = (obj: any): any => {
  if (Array.isArray(obj)) {
    return obj.map((item) => convertToSnakeCase(item));
  }

  if (obj !== null && typeof obj === 'object') {
    return Object.entries(obj).reduce((acc: Record<string, any>, [key, value]) => {
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

export default convert;
