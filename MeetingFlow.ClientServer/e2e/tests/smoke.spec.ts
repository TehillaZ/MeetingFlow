import { test, expect } from "@playwright/test";

/**
 * A worked example. This test should pass against the current code.
 * Use it as the template for the tests in the homework.
 */
// test("the meetings page loads", async ({ page }) => {
//   // Arrange — go to the page under test.
//   await page.goto("/");

//   // Assert — a web-first assertion waits for the condition instead of sleeping.
//   await expect(page.getByRole("heading", { name: "Meetings", level: 1 })).toBeVisible();
//   await expect(page.getByRole("link", { name: "Frontend Architecture Summit" })).toBeVisible();
// });

// This test is unworked because the meeting dropdown are not connected to the label of meeting. 
// You will need to fix the code in the CreateRegistrationPage.tsx file to make this test pass.
test("user can open the meeting dropdown", async ({ page }) => {
  // Arrange — go to the registration page.
  await page.goto("/register");

  // Act — click on Meeting.
  await page.getByRole("combobox").first().click();

  // Assert — the dropdown options should be visible.
  await expect(
    page.getByRole("option")
  ).toHaveCount(4);
});
