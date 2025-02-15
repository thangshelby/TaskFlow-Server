import { setupApp } from '~/app';

setupApp()
  .then((app) => {
    app.listen(process.env.SERVER_PORT, () => {
      console.log(`🚀 Server is running on http://localhost:${process.env.SERVER_PORT}`);
    });
  })
  .catch((err) => {
    console.error('❌ Error starting the server:', err);
    process.exit(1);
  });
