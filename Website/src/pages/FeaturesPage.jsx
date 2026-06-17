import { motion } from 'framer-motion'
import './FeaturesPage.css'

const features = [
  {
    title: 'Server-backed chat',
    body: 'General questions route through the configured Smartitecture backend, with local fallback when the server is unavailable.'
  },
  {
    title: 'Performance checks',
    body: 'Reads CPU, memory, process count, and top memory users so the assistant can explain why a PC may feel slow.'
  },
  {
    title: 'Security checks',
    body: 'Integrates with Microsoft Defender for scan status and quick scan requests on supported Windows machines.'
  },
  {
    title: 'Network and battery status',
    body: 'Reports active adapters, IP addresses, link speed, and current battery charge when Windows exposes that data.'
  },
  {
    title: 'Application launching',
    body: 'Launches common Windows tools and installed Start Menu apps from natural-language requests.'
  },
  {
    title: 'Voice input',
    body: 'Uses Windows speech recognition and microphone permission for spoken prompts where available.'
  }
]

function FeaturesPage() {
  return (
    <div className="features-page">
      <section className="features-hero">
        <div className="container">
          <motion.div
            className="features-hero-copy"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <p className="eyebrow">Current beta capabilities</p>
            <h1>Built around one assistant, not separate modes.</h1>
            <p>
              Smartitecture chooses the right path automatically: backend AI for broad answers,
              local tools for PC diagnostics, and guarded automation for system actions.
            </p>
          </motion.div>
        </div>
      </section>

      <section className="section feature-details">
        <div className="container feature-grid">
          {features.map((feature) => (
            <motion.article
              className="feature-card"
              key={feature.title}
              initial={{ opacity: 0, y: 18 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.35 }}
            >
              <h2>{feature.title}</h2>
              <p>{feature.body}</p>
            </motion.article>
          ))}
        </div>
      </section>

      <section className="section limitations-section">
        <div className="container limitations-grid">
          <div>
            <p className="eyebrow">Beta boundaries</p>
            <h2>Clear limits make this safer to test.</h2>
          </div>
          <ul>
            <li>Windows is the only supported desktop platform for this beta.</li>
            <li>Mac and Linux would need separate app shells and system connectors.</li>
            <li>Destructive or sensitive actions should require confirmation before execution.</li>
            <li>Public downloads still need hosted artifacts and release notes.</li>
          </ul>
        </div>
      </section>
    </div>
  )
}

export default FeaturesPage
