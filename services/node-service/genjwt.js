// eslint-disable-next-line @typescript-eslint/no-require-imports
const jwt = require("jsonwebtoken");

// eslint-disable-next-line @typescript-eslint/no-require-imports
const fs = require("fs");

// Load your private key
const privateKey = fs.readFileSync("../../private_key.pem", "utf8"); // Save your private key in a file named 'private.pem'

// JWT Payload
const payload = {
  sub: "1234567890", // User ID or subject
  name: "John Doe",
  iat: Math.floor(Date.now() / 1000), // Issued at
  exp: Math.floor(Date.now() / 1000) + 60 * 60, // Expires in 1 hour
  iss: "127.0.0.1", // MUST match the issuer in your Envoy config
  role: "user_role"
};

// Generate the token
const token = jwt.sign(payload, privateKey, { algorithm: "RS256" });

console.log("Generated JWT Token:");
console.log(token);
