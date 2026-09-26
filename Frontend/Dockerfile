# Etapa 1: compilar o Angular
FROM node:24-alpine AS build

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build


# Etapa 2: servir com Nginx
FROM nginx:1.29-alpine

COPY --from=build /app/dist/Frontend/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]