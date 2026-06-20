import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import './HomePage.css'

const capabilities = [
  ['Ask anything', 'General questions route through the Smartitecture backend when configured.'],
  ['Understand your PC', 'Check memory, battery, Defender status, network adapters, and running processes.'],
  ['Open apps faster', 'Launch Windows tools and installed Start Menu apps from plain language.'],
  ['Stay in control', 'Sensitive actions stay guarded and local diagnostics remain transparent.']
]

function HomePage() {
  return (
    <div className="home-page">
      <section className="hero-showcase">
        <div className="container hero-grid">
          <motion.div
            className="hero-copy"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <p className="eyebrow">Windows AI system assistant</p>
            <h1>Smartitecture</h1>
            <p className="hero-lede">
              A desktop assistant that helps people understand what is happening on their PC,
              get AI answers, run local checks, and take action without digging through Windows menus.
            </p>
            <div className="hero-actions">
              <Link to="/download" className="button large">Download beta</Link>
              <Link to="/features" className="button secondary large">Explore features</Link>
            </div>
          </motion.div>

          <motion.div
            className="hero-status-panel"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45, delay: 0.1 }}
          >
            <div className="status-header">
              <span></span>
              <strong>Beta status</strong>
            </div>
            <div className="status-metric">
              <span>Version</span>
              <strong>1.0.0.1 beta</strong>
            </div>
            <div className="status-metric">
              <span>Backend</span>
              <strong>Render connected</strong>
            </div>
            <div className="status-metric">
              <span>Platform</span>
              <strong>Windows 10 / 11</strong>
            </div>
            <Link to="/download" className="status-link">View download options</Link>
          </motion.div>
        </div>
      </section>

      <section className="section capability-section">
        <div className="container">
          <div className="section-intro">
            <p className="eyebrow">What it does</p>
            <h2>One assistant for chat, diagnostics, app control, and safe automation.</h2>
          </div>
          <div className="capability-grid">
            {capabilities.map(([title, text]) => (
              <article className="capability-card" key={title}>
                <h3>{title}</h3>
                <p>{text}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="section demo-section">
        <div className="container demo-grid">
          <div className="demo-panel">
            <div className="mock-topbar">
              <span>AI Server connected</span>
              <span>Local tools ready</span>
            </div>
            <div className="mock-thread">
              <p className="mock-user">Why is my PC slow?</p>
              <p className="mock-assistant">
                Memory usage is elevated. I would close unused editor windows and heavy browser tabs first.
              </p>
              <p className="mock-user">Did Defender find anything?</p>
              <p className="mock-assistant">
                The last scan finished, and Defender did not report any threats.
              </p>
            </div>
          </div>
          <div>
            <p className="eyebrow">Built for real use</p>
            <h2>Not just hardcoded replies.</h2>
            <p>
              Smartitecture combines a hosted AI backend with local Windows connectors, so it can answer
              broad questions and still inspect the machine when the user asks about performance,
              battery, network, security, or installed apps.
            </p>
            <Link to="/download" className="button">Get the beta</Link>
          </div>
        </div>
      </section>
    </div>
  )
}

export default HomePage
