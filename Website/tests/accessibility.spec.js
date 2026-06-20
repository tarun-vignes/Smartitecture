import { test, expect } from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'

const pages = [
  { name: 'Home', path: '/' },
  { name: 'Features', path: '/features' },
  { name: 'Download', path: '/download' },
  { name: 'FAQ', path: '/faq' },
  { name: 'About', path: '/about' }
]

for (const pageInfo of pages) {
  test(`${pageInfo.name} page has no WCAG A/AA violations`, async ({ page }) => {
    await page.goto(pageInfo.path)
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(700)

    const results = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze()

    expect(formatViolations(results.violations)).toEqual([])
  })
}

function formatViolations(violations) {
  return violations.map((violation) => ({
    id: violation.id,
    impact: violation.impact,
    description: violation.description,
    nodes: violation.nodes.map((node) => ({
      target: node.target,
      failureSummary: node.failureSummary
    }))
  }))
}
