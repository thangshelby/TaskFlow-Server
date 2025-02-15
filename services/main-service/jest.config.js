module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  roots: ['./src/tests'],
  moduleNameMapper: {
    '^~/(.*)$': '<rootDir>/src/$1'
  }
};
