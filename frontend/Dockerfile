# frontend/Dockerfile.dev
FROM node:20-alpine

WORKDIR /app

# Instala o Angular CLI globalmente
RUN npm install -g @angular/cli

# Copia só o package.json para aproveitar cache
COPY package*.json ./

# Instala dependências
RUN npm install

# Copia o restante do código (será sobreposto pelo volume no Compose)
COPY . .

# Exponha a porta padrão do ng serve
EXPOSE 4200

# Comando de desenvolvimento:  
# --host 0.0.0.0 para aceitar conexões externas  
# --poll 2000 em Alpine (ajusta o polling do watch)
CMD ["ng", "serve", "--host", "0.0.0.0", "--poll", "2000"]
