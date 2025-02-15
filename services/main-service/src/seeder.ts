import mysql from 'mysql2/promise';
import { faker } from '@faker-js/faker';

const BATCH_SIZE = 10000; // Insert 10,000 rows at a time
const TOTAL_RECORDS = 1000000; // Total records to insert

const main = async () => {
  // 🔹 1. Connect to MySQL
  const connection = await mysql.createConnection({
    host: 'localhost',
    user: 'root',
    password: 'Vudinhan123.',
    database: 'testdb'
  });

  console.log('✅ Connected to MySQL');

  // 🔹 2. Prepare INSERT query (remove `id` if it's AUTO_INCREMENT)
  const insertQuery = `INSERT INTO user_model (id,name, email, password) VALUES ?`;

  for (let i = 0; i < TOTAL_RECORDS / BATCH_SIZE; i++) {
    const data: [string, string, string, string][] = []; // ✅ Fixed Type

    for (let j = 0; j < BATCH_SIZE; j++) {
      data.push([
        faker.string.uuid(),
        faker.person.fullName(), // ✅ Generates random full name
        faker.internet.email({ firstName: faker.string.alphanumeric(5) + j }), // ✅ Unique email
        faker.internet.password({ length: 10 })
      ]);
    }

    await connection.query(insertQuery, [data]);
    console.log(`Inserted ${(i + 1) * BATCH_SIZE} rows...`);
  }

  console.log('🎉 1,000,000 rows inserted successfully!');
  await connection.end();
};

// Run the function
main().catch(console.error);
