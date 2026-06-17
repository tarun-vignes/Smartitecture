import { motion } from 'framer-motion'
import './AboutPage.css'

const milestones = [
  'Unified the assistant into one chat experience.',
  'Connected the desktop app to a hosted backend for broader AI answers.',
  'Added local Windows diagnostics for performance, battery, network, and security checks.',
  'Built signed beta packaging and clean-machine QA scripts.'
]

function AboutPage() {
  return (
    <div className="about-page">
      <section className="about-hero">
        <div className="container">
          <motion.div
            className="about-hero-copy"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <p className="eyebrow">About the project</p>
            <h1>Smartitecture makes PC help feel direct and understandable.</h1>
            <p>
              The project is focused on a practical Windows assistant: ask normal questions,
              run local checks, launch apps, and get explanations that connect to the current system state.
            </p>
          </motion.div>
        </div>
      </section>

      <section className="section about-mission">
        <div className="container about-grid">
          <div>
            <h2>Mission</h2>
            <p>
              Smartitecture is being built for people who want a useful computer assistant, not a demo
              that only answers hardcoded prompts. The beta combines a hosted AI service with local
              Windows tools so users can ask both general and machine-specific questions.
            </p>
          </div>
          <div>
            <h2>Current progress</h2>
            <ul className="milestone-list">
              {milestones.map((item) => (
                <li key={item}>{item}</li>
              ))}
            </ul>
          </div>
        </div>
      </section>

      <section className="section about-values">
        <div className="container values-grid">
          <article>
            <h3>Honest scope</h3>
            <p>Only claim features that are implemented or clearly marked as roadmap work.</p>
          </article>
          <article>
            <h3>Safety</h3>
            <p>Keep destructive automation guarded and make system actions understandable.</p>
          </article>
          <article>
            <h3>Production path</h3>
            <p>Use hosted backend keys, signed packages, QA scripts, and clear beta release notes.</p>
          </article>
        </div>
      </section>
    </div>
  )
}

export default AboutPage
