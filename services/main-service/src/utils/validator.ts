import { Request } from 'express';
import { z } from 'zod';

const paginationSchema = z.object({
  page: z.number().int().min(1).default(1),
  limit: z.number().int().min(1).max(1000000).default(10)
});

export function validatePaginationParams(req: Request) {
  const { page = 1, limit } = req.query;
  const result = paginationSchema.safeParse({ page: Number(page), limit: Number(limit) });

  if (!result.success) {
    throw new Error('Invalid pagination parameters: Page must be >= 1 and Limit must be between 1-1000000.');
  }

  return result.data;
}
