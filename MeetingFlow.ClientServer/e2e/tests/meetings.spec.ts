import { test, expect } from "@playwright/test";

test("public catalogue only shows published meetings", async ({ page }) => {

  await page.goto("/")

   await expect(
    page.getByRole("heading", { level: 3 })
   ).toHaveCount(3);

   await expect(
    page.getByRole("link", { name: "Product Engineering Meetup" })
    ).toBeVisible();

    await expect(
    page.getByRole("link", { name: "Frontend Architecture Summit" })
    ).toBeVisible();

    await expect(
    page.getByRole("link", { name: "Cloud Integration Day" })
    ).toBeVisible();

    await expect(
    page.getByRole("link", { name: "Distributed Systems Workshop" })
  ).toHaveCount(0);

  await expect(
    page.getByRole("link", { name: "AI Tools for Developers" })
  ).toHaveCount(0);
})