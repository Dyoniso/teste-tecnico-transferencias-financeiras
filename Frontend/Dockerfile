FROM node:24-alpine AS build

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build

# Alguns builds com SSR geram index.csr.html.
# Nesse caso, transformamos no index.html que o Nginx espera.
RUN if [ -f /app/dist/Frontend/browser/index.csr.html ]; then \
      cp /app/dist/Frontend/browser/index.csr.html \
         /app/dist/Frontend/browser/index.html; \
    fi


FROM nginx:1.29-alpine

# Remove somente os arquivos de exemplo da imagem Nginx.
RUN rm -f \
    /usr/share/nginx/html/index.html \
    /usr/share/nginx/html/50x.html

COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build \
    /app/dist/Frontend/browser/ \
    /usr/share/nginx/html/

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]