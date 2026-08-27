import { test, expect } from "@playwright/test";

test("user can register for a meeting", async ({ page }) => {
  await page.goto("/register");

  await expect(
    page.getByRole("heading", { name: "Register for a Meeting" })
  ).toBeVisible();

  await page.getByRole("combobox").nth(0).selectOption({ index: 1 });

  await page.getByLabel("Your Name").fill("Test User");

  await page
    .getByLabel("Your Email")
    .fill(`e2e-${Date.now()}@meetingflow.test`);

  await page.getByRole("combobox").nth(1).selectOption({ index: 1 });

  await page.getByRole("button", { name: "Register" }).click();

  await expect(
    page.getByText("You have successfully registered for the meeting.")
  ).toBeVisible();
});
