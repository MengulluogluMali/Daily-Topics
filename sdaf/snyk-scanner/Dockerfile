FROM snyk/snyk:node

# Install express for the wrapper script
WORKDIR /app
COPY package.json .
RUN npm install

COPY index.js .

EXPOSE 3002

CMD ["npm", "start"]
