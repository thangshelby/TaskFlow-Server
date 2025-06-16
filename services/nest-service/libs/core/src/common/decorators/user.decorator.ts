import { createParamDecorator, ExecutionContext } from '@nestjs/common';

export const UserMetadata = createParamDecorator((_: unknown, ctx: ExecutionContext) => {
  const metadata = ctx.switchToRpc().getContext();
  const userId = metadata.get('userId')?.[0];
  const userRole = metadata.get('userRole')?.[0];
  return {
    userId,
    userRole,
  };
});
