import pino from 'pino';
import pinoHttp from 'pino-http';
import 'pino-pretty';

export const log = pino({
  level: process.env.LOG_LEVEL || 'info',
  transport: {
    target: 'pino-pretty',
    options: {
      colorize: true, // Enable colors
      singleLine: false, // Easier reading
    },
  },
});

export const logHttp = pinoHttp({
  transport: {
    target: 'pino-pretty',
    options: {
      colorize: true, // Enable colors
      singleLine: false, // Easier reading
    },
  },
  serializers: {
    req(req) {
      return {
        ...req,
        url: req.url.length > 200 ? `${req.url.slice(0, 200)}...(truncated)` : req.url,
      };
    },
  },
});
