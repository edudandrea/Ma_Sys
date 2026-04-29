import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import { join } from 'node:path';
import { Readable } from 'node:stream';

const browserDistFolder = join(import.meta.dirname, '../browser');
const backendUrl = (process.env['BACKEND_URL'] || 'http://localhost:5167').replace(/\/$/, '');

const app = express();
const angularApp = new AngularNodeAppEngine();

app.get('/health', (_req, res) => {
  res.status(200).json({ status: 'ok' });
});

app.use('/api', async (req, res, next) => {
  try {
    const targetUrl = `${backendUrl}${req.originalUrl}`;
    const headers = new Headers();

    Object.entries(req.headers).forEach(([key, value]) => {
      if (key.toLowerCase() === 'host' || typeof value === 'undefined') {
        return;
      }

      if (Array.isArray(value)) {
        value.forEach((item) => headers.append(key, item));
        return;
      }

      headers.set(key, value);
    });

    const init: RequestInit & { duplex?: 'half' } = {
      method: req.method,
      headers,
      redirect: 'manual',
    };

    if (req.method !== 'GET' && req.method !== 'HEAD') {
      init.body = req as unknown as BodyInit;
      init.duplex = 'half';
    }

    const response = await fetch(targetUrl, init);

    res.status(response.status);
    response.headers.forEach((value, key) => {
      if (key.toLowerCase() === 'transfer-encoding') {
        return;
      }

      res.setHeader(key, value);
    });

    if (!response.body) {
      res.end();
      return;
    }

    Readable.fromWeb(response.body as any).pipe(res);
  } catch (error) {
    next(error);
  }
});

/**
 * Serve static files from /browser
 */
app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

/**
 * Handle all other requests by rendering the Angular application.
 */
app.use((req, res, next) => {
  angularApp
    .handle(req)
    .then((response) =>
      response ? writeResponseToNodeResponse(response, res) : next(),
    )
    .catch(next);
});

/**
 * Start the server if this module is the main entry point, or it is ran via PM2.
 * The server listens on the port defined by the `PORT` environment variable, or defaults to 4000.
 */
if (isMainModule(import.meta.url) || process.env['pm_id']) {
  const port = process.env['PORT'] || 4000;
  app.listen(port, (error) => {
    if (error) {
      throw error;
    }

    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

/**
 * Request handler used by the Angular CLI (for dev-server and during build) or Firebase Cloud Functions.
 */
export const reqHandler = createNodeRequestHandler(app);
