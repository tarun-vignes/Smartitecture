import { useState } from 'react'
import { motion } from 'framer-motion'
import './FAQPage.css'

const questions = [
  {
    question: 'What is Smartitecture?',
    answer: 'Smartitecture is a Windows desktop assistant that combines backend AI answers with local PC diagnostics, app launching, and safe automation.'
  },
  {
    question: 'Is this ready for public release?',
    answer: 'It is ready for controlled beta testing. The app, backend, signed package, and QA script are working, but public download hosting and a tester feedback loop still need to be finalized.'
  },
  {
    question: 'Does it work on Mac or Linux?',
    answer: 'Not in the current beta. The app is WPF and uses Windows-specific APIs for Defender, speech recognition, app launching, battery, and system diagnostics.'
  },
  {
    question: 'Does it need the internet?',
    answer: 'General AI answers need the Smartitecture backend. Local diagnostics such as performance, battery, network, and Defender status can still run locally.'
  },
  {
    question: 'What languages are supported?',
    answer: 'The interface includes many localization files, but beta validation should focus on English first. Full translation quality review is still a release task before claiming broad language support.'
  },
  {
    question: 'Is the backend secure?',
    answer: 'The production backend requires an API key and keeps provider secrets on the server. Do not ship Gemini or provider keys inside the desktop app.'
  },
  {
    question: 'Can it scan for malware?',
    answer: 'On Windows, it can request Microsoft Defender scan actions and read Defender scan status. It does not replace Windows Security or a full antivirus product.'
  },
  {
    question: 'What should beta testers try?',
    answer: 'Ask general questions, check performance, check battery, view Defender scan results, scan the PC, launch common apps, and test the Settings backend connection.'
  }
]

function FAQPage() {
  const [openQuestion, setOpenQuestion] = useState(questions[0].question)

  return (
    <div className="faq-page">
      <section className="faq-hero">
        <div className="container">
          <motion.div
            className="hero-content"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <p className="eyebrow">FAQ</p>
            <h1>Beta questions, answered plainly.</h1>
            <p>Current scope, limits, and testing guidance for the Smartitecture Windows beta.</p>
          </motion.div>
        </div>
      </section>

      <section className="section faq-main">
        <div className="container faq-container">
          {questions.map((item) => {
            const isOpen = openQuestion === item.question
            return (
              <article className={`faq-item ${isOpen ? 'expanded' : ''}`} key={item.question}>
                <button
                  className="faq-question"
                  onClick={() => setOpenQuestion(isOpen ? '' : item.question)}
                  aria-expanded={isOpen}
                >
                  {item.question}
                  <span className="toggle-icon">{isOpen ? '-' : '+'}</span>
                </button>
                {isOpen && (
                  <div className="faq-answer">
                    <p>{item.answer}</p>
                  </div>
                )}
              </article>
            )
          })}
        </div>
      </section>
    </div>
  )
}

export default FAQPage
