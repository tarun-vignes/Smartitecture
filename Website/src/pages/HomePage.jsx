import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import './HomePage.css'

const capabilities = [
  {
    title: 'AI answers on demand',
    text: 'Connects to the Smartitecture backend for general questions, explanations, and follow-up reasoning.'
  },
  {
    title: 'Local PC diagnostics',
    text: 'Checks performance, memory pressure, battery status, network adapters, and Windows Defender results.'
  },
  {
    title: 'App and tool launching',
    text: 'Opens common Windows apps and Start Menu apps from plain-language requests.'
  },
  {
    title: 'Safety-first automation',
    text: 'Keeps sensitive actions guarded and explains what it is doing before touching system tools.'
  }
]

function HomePage() {
  return (
    <div className="home-page">
      <section className="beta-hero">
        <div className="container beta-hero-grid">
          <motion.div
            className="beta-hero-copy"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <p className="eyebrow">Windows beta now available</p>
            <h1>Smartitecture</h1>
            <p className="hero-summary">
              A desktop assistant that combines cloud AI with local Windows diagnostics, app control,
              and clear explanations for everyday computer problems.
            </p>
            <div className="hero-actions">
              <Link to="/download" className="button large">Download beta</Link>
              <Link to="/features" className="button secondary large">See features</Link>
            </div>
          </motion.div>

          <motion.div
            className="product-panel"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45, delay: 0.1 }}
            aria-label="Smartitecture assistant preview"
          >
            <div className="mock-window-bar">
              <span></span>
              <span></span>
              <span></span>
            </div>
            <div className="mock-toolbar">
              <span>AI Server connected</span>
              <span>History</span>
            </div>
            <div className="mock-chat">
              <p className="mock-user">Why is my PC slow?</p>
              <p className="mock-assistant">
                Memory usage is elevated. Close unused editor windows, browser tabs, or heavy apps first.
              </p>
              <p className="mock-user">Scan results?</p>
              <p className="mock-assistant">
                Defender finished the last scan and did not report any threats.
              </p>
            </div>
          </motion.div>
        </div>
      </section>

      <section className="section proof-strip">
        <div className="container proof-grid">
          <div>
            <strong>Version</strong>
            <span>1.0.0.1 beta</span>
          </div>
          <div>
            <strong>Platform</strong>
            <span>Windows 10 and Windows 11</span>
          </div>
          <div>
            <strong>Backend</strong>
            <span>Render-hosted Smartitecture API</span>
          </div>
        </div>
      </section>

      <section className="section capability-section">
        <div className="container">
          <div className="section-heading">
            <p className="eyebrow">What it does today</p>
            <h2>One assistant for questions, diagnostics, and safe automation.</h2>
          </div>
          <div className="capability-grid">
            {capabilities.map((item) => (
              <article className="capability-card" key={item.title}>
                <h3>{item.title}</h3>
                <p>{item.text}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="section beta-status-section">
        <div className="container beta-status">
          <div>
            <p className="eyebrow">Release status</p>
            <h2>Ready for beta testers, not a broad public launch yet.</h2>
            <p>
              The Windows package, backend connection, and clean-machine QA flow are in place. The next
              release step is a public download page with clear beta instructions and a small tester group.
            </p>
          </div>
          <Link to="/download" className="button large">Get the beta</Link>
        </div>
      </section>
    </div>
  )
}

export default HomePage
