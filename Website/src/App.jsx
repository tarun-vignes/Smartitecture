import { useEffect } from 'react'
import { Routes, Route, Link, useLocation } from 'react-router-dom'
import Header from './components/Header'
import Footer from './components/Footer'
import HomePage from './pages/HomePage'
import AboutPage from './pages/AboutPage'
import DownloadPage from './pages/DownloadPage'
import FeaturesPage from './pages/FeaturesPage'
import FAQPage from './pages/FAQPage'
import './App.css'

function App() {
  const { pathname } = useLocation()
  useEffect(() => {
    window.scrollTo(0, 0)
    const names = { '/': 'Your PC, a little less complicated', '/features': 'How it helps', '/download': 'Windows beta', '/faq': 'Common questions', '/about': 'Our story' }
    document.title = `${names[pathname] || 'Page not found'} | Smartitecture`
  }, [pathname])
  return (
    <div className="app">
      <a className="skip-link" href="#main-content">Skip to main content</a>
      <Header />
      <main className="main-content" id="main-content" tabIndex={-1}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/download" element={<DownloadPage />} />
          <Route path="/features" element={<FeaturesPage />} />
          <Route path="/faq" element={<FAQPage />} />
          <Route path="*" element={<section className="container not-found"><p className="eyebrow">A wrong turn</p><h1>Let?s get you back.</h1><p>We couldn?t find that page.</p><Link className="button" to="/">Back to home</Link></section>} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}

export default App
