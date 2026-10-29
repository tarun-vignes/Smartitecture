import { test, expect } from '@playwright/test'

test('visitors can explore examples without confusing them with a live PC check', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Peace of mind' }).click()
  await expect(page.getByText('Is my computer protected?', { exact: true })).toBeVisible()
  await expect(page.getByText('Illustrative conversation', { exact: false })).toBeVisible()
  await page.getByRole('button', { name: 'Everyday shortcuts' }).click()
  await expect(page.getByText('Open calculator', { exact: true })).toBeVisible()
  await page.getByRole('link', { name: 'Try the Windows beta' }).click()
  await expect(page).toHaveURL(/\/download$/)
  await expect(page.getByText('This is an early beta, not a finished release.')).toBeVisible()
  await page.getByText('File details for advanced users', { exact: true }).click()
  await expect(page.getByText('Installer SHA256', { exact: true })).toBeVisible()
  for (const label of ['Download Windows installer', 'Download portable version']) {
    const address = await page.getByRole('link', { name: label }).getAttribute('href')
    const response = await page.request.head(address)
    expect(response.ok()).toBeTruthy()
  }
})

test('phone visitors can navigate and read every page without horizontal scrolling', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/')
  await page.getByRole('button', { name: 'Open navigation menu' }).click()
  await page.getByRole('navigation').getByRole('link', { name: 'Questions' }).click()
  await expect(page).toHaveURL(/\/faq$/)
  await expect(page.getByRole('button', { name: 'Open navigation menu' })).toHaveAttribute('aria-expanded', 'false')
  await page.getByRole('button', { name: 'Does it work without the internet?' }).click()
  await expect(page.getByRole('region', { name: 'Does it work without the internet?' })).toBeVisible()
  for (const route of ['/', '/features', '/download', '/faq', '/about']) {
    await page.goto(route)
    const fits = await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)
    expect(fits, `${route} should fit a phone screen`).toBeTruthy()
  }
})
