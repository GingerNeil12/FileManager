declare const AUTH0_DOMAIN: string;
declare const AUTH0_CLIENT_ID: string;
declare const AUTH0_AUDIENCE: string;
declare const API_BASE_URL: string;

export const environment = {
  production: true,
  appName: 'filemanager-frontend',
  appVersion: '1.3.0',
  apiBaseUrl: API_BASE_URL,
  auth0Domain: AUTH0_DOMAIN,
  auth0ClientId: AUTH0_CLIENT_ID,
  auth0Audience: AUTH0_AUDIENCE,
};
