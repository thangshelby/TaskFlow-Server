import http from "k6/http";
import { check } from "k6";
import { Counter } from "k6/metrics";

export const options = {
  vus: 1, // 20 virtual users cùng lúc
  duration: "10s", // chạy trong 10 giây
  thresholds: {
    http_req_failed: ["rate<0.2"], // không quá 20% request fail
  },
};

// Custom metric để đếm số request bị 429
export const rateLimitedRequests = new Counter("rate_limited_requests");

export default function () {
  const url = "http://localhost:8081/api/v1/issues/list-issue";

  const payload = JSON.stringify({
    project_id: "68a849a23a4bc70d852d553b",
  });

  const params = {
    headers: {
      "Content-Type": "application/json",
      Cookie:
        "token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2N2Y0OTNhNzc4MTI1M2JiMmFkODRiZDQiLCJuYW1lIjoidXNlckBnbWFpbC5jb20iLCJpYXQiOjE3NjYyODE4MzcsImV4cCI6MTc2NjM2ODIzNywiaXNzIjoiMTI3LjAuMC4xIiwicm9sZSI6IlVzZXIiLCJ1c2VySWQiOiI2N2Y0OTNhNzc4MTI1M2JiMmFkODRiZDQiLCJpc1ZlcmlmaWVkIjoidHJ1ZSIsIm5iZiI6MTc2NjI4MTgzN30.roJ9KVWPWouF0Du_fX4Wsfbb-Gye3tlt4M0dfUQeT3PpATuITepsDxWq0qkHtiZuglWMB9WeIsQADCOCdGt3-eOz-qBDcix3CJntzPXAsQVGWIwLzFH5JH4zK3ALNknfaq57JfErKgLYxMt2nQfcuf-x38s8R6WBpj-gMFPhuIJaViEflLPDReszto0AZlg-VVVQ1mK24BQrJJDvB_8A4X8S094JOIRn5smgWCkWsBWVHNUur-8GbR9ayq27lJRYXmXA3LjyJXiJVX_1hvUYaJy4coNcaQMZom4k5CAH02GDQBpQ77WR-w0Jr6dYaIHJEBgHF5of6V4Wvx8ABPFRqg; Path=/; Secure; HttpOnly;",
      // nếu test theo user/token
      // Authorization: "Bearer test-token",
    },
  };

  const res = http.post(url, payload, params);

  // Check kết quả
  const ok = check(res, {
    "status is 200 or 429": (r) => r.status === 200 || r.status === 429,
  });

  // Nếu bị rate limit thì đếm
  if (res.status === 429) {
    rateLimitedRequests.add(1);
  }
}
