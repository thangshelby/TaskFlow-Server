import dotenv from "dotenv";
import mongoose from "mongoose";
dotenv.config();

export const connectDatabase = async () => {
  await mongoose
    .connect(process.env.CONNECTION_STRING)
    .then(() => {
      console.log("Connected to database");
    })
    .catch(() => {
      console.log("Not able to connect to database");
    });

  return mongoose.connection;
};
