import { test, expect } from '@playwright/test';

test('homepage has Copilot Proxy title and description', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle(/Copilot Proxy/i);
  await expect(page.locator('body')).toContainText(/GitHub Copilot/i);
});
