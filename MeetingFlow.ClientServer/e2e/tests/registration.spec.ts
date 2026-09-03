import { test, expect } from "@playwright/test";

test("user can register for a meeting", async ({ page }) => {
  await page.goto("/register");

  await expect(
    page.getByRole("heading", { name: "Register for a Meeting" })
  ).toBeVisible();

  await page.getByLabel("Meeting").selectOption({ index: 1 });

  await page.getByLabel("Your Name").fill("Test User");

  await page
    .getByLabel("Your Email")
    .fill(`e2e-${Date.now()}@meetingflow.test`);

  await page.getByLabel("Ticket Type").selectOption({ label: "VIP" });

  await page.getByRole("button", { name: "Register" }).click();

  await expect(
    page.getByText("Registration created successfully!")
  ).toBeVisible();
});