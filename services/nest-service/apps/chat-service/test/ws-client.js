// const WebSocket = require('ws');

// // Test user token
// const token = 'YOUR_JWT_TOKEN_HERE';
// const ws = new WebSocket('ws://localhost:5003', {
//   headers: {
//     Cookie: `token=${token}`,
//   },
// });

// ws.on('open', () => {
//   console.log('Connected to WebSocket server');
// });

// ws.on('message', (data) => {
//   try {
//     const message = JSON.parse(data.toString());
//     console.log('Received:', message);

//     // If connection is successful, try to join a room
//     if (message.event === 'connection_ack') {
//       ws.send(
//         JSON.stringify({
//           event: 'joinRoom',
//           data: {
//             roomId: 'TEST_ROOM_ID',
//           },
//         }),
//       );
//     }

//     // If joined room successfully, send a test message
//     if (message.event === 'messageHistory') {
//       ws.send(
//         JSON.stringify({
//           event: 'sendMessage',
//           data: {
//             roomId: 'TEST_ROOM_ID',
//             content: 'Test message',
//             type: 'text',
//           },
//         }),
//       );
//     }
//   } catch (err) {
//     console.error('Failed to parse message:', data.toString());
//   }
// });

// ws.on('error', console.error);
// ws.on('close', () => console.log('Connection closed'));
