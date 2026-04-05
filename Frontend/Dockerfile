FROM node:alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .

ARG AUTH0_DOMAIN
ARG AUTH0_CLIENT_ID
ARG AUTH0_AUDIENCE
ARG API_BASE_URL

RUN npm run build -- \
  --define "AUTH0_DOMAIN='${AUTH0_DOMAIN}'" \
  --define "AUTH0_CLIENT_ID='${AUTH0_CLIENT_ID}'" \
  --define "AUTH0_AUDIENCE='${AUTH0_AUDIENCE}'" \
  --define "API_BASE_URL='${API_BASE_URL}'"

FROM nginx:alpine AS runtime
COPY --from=build /app/dist/file-manager/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
