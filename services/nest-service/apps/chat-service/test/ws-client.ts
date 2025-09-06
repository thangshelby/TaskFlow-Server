// import WebSocket from 'ws';

// const TEST_CONFIG = {
//   token: 'YOUR_JWT_TOKEN_HERE',
//   roomId: 'TEST_ROOM_ID',
//   serverUrl: 'ws://localhost:5003',
// };

// class TestClient {
//   private ws: WebSocket;

//   constructor() {
//     this.ws = new WebSocket(TEST_CONFIG.serverUrl, {
//       headers: {
//         Cookie: `token=${TEST_CONFIG.token}`,
//       },
//     });

//     this.setupEventHandlers();
//   }

//   private setupEventHandlers() {
//     this.ws.on('open', this.handleOpen.bind(this));
//     this.ws.on('message', this.handleMessage.bind(this));
//     this.ws.on('error', this.handleError.bind(this));
//     this.ws.on('close', this.handleClose.bind(this));
//   }

//   private handleOpen() {
//     console.log('Connected to WebSocket server');
//   }

//   private handleMessage(data: WebSocket.Data) {
//     try {
//       const message = JSON.parse(data.toString());
//       console.log('Received:', message);

//       // Handle different message types
//       switch (message.event) {
//         case 'connection_ack':
//           this.joinRoom();
//           break;
//         case 'messageHistory':
//           this.sendTestMessage();
//           break;
//       }
//     } catch (err) {
//       console.error('Failed to parse message:', data.toString());
//     }
//   }

//   private handleError(error: Error) {
//     console.error('WebSocket error:', error);
//   }

//   private handleClose() {
//     console.log('Connection closed');
//   }

//   private joinRoom() {
//     console.log('Joining room:', TEST_CONFIG.roomId);
//     this.ws.send(
//       JSON.stringify({
//         event: 'joinRoom',
//         data: {
//           roomId: TEST_CONFIG.roomId,
//         },
//       }),
//     );
//   }

//   private sendTestMessage() {
//     console.log('Sending test message');
//     this.ws.send(
//       JSON.stringify({
//         event: 'sendMessage',
//         data: {
//           roomId: TEST_CONFIG.roomId,
//           content: 'Test message',
//           type: 'text',
//         },
//       }),
//     );
//   }
// }

// // Start test client
// console.log('Starting WebSocket test client...');
// new TestClient();
