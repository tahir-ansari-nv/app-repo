const createApp = require('./app');
const config = require('./config');

const app = createApp();

const PORT = config.server.port;

app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
  console.log(`Environment: ${config.server.env}`);
  console.log(`Login endpoint: http://localhost:${PORT}/api/auth/login`);
  console.log(`Health check: http://localhost:${PORT}/api/auth/health`);
});
