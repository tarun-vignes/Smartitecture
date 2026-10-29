import { useState } from 'react'
import './FAQPage.css'

const questions = [
  { question: 'What does Smartitecture do?', answer: 'It helps you understand your Windows PC, check things such as memory and battery, open apps, and get basic computer help in one place.' },
  { question: 'Do I need to know about computers?', answer: 'You can ask in everyday language, such as “Why is my PC slow?” The beta installation can still take a few extra steps. See the download page before you start.' },
  { question: 'Which computers can use it?', answer: 'The current beta is for 64-bit Windows 10 and Windows 11 PCs. It isn’t available for Mac, Linux, phones, or tablets.' },
  { question: 'Does it work without the internet?', answer: 'PC checks and app shortcuts can work offline. Broader AI conversations need an internet connection and access to the online AI service, which is still being improved.' },
  { question: 'Is it finished?', answer: 'Not yet. This is early-access software for people who are comfortable trying an app while it is still being developed. Expect rough edges and occasional problems.' },
  { question: 'Will it replace my antivirus?', answer: 'No. Smartitecture can show information from Microsoft Defender and request supported scans. Keep Windows Security or your existing antivirus enabled.' },
  { question: 'Where does my information go?', answer: 'PC checks run on your computer. Questions sent to online AI are processed by the connected service. Chat history and settings are saved on your PC. Avoid including passwords or private information in messages.' },
  { question: 'Why does Windows show a warning when I install?', answer: 'The beta installer uses a development signing certificate, so Windows may block it. A simpler public installer is still in progress. Don’t turn off Windows security protections to install it.' },
  { question: 'How can I report a problem?', answer: 'Use the Report a problem link at the bottom of this site. It opens GitHub, where you can describe what happened. A GitHub account is required. Please leave passwords and other private details out of your report.' }
]

function FAQPage() {
  const [openQuestion, setOpenQuestion] = useState(questions[0].question)

  return (
    <div className="faq-page">
      <section className="page-intro">
        <div className="container">
          <div className="hero-content"><p className="eyebrow">A little clarity</p><h1>Questions? Start here.</h1><p>What it does, what you need, and what to expect from the beta.</p></div>
        </div>
      </section>

      <section className="section faq-main">
        <div className="container faq-container">
          {questions.map((item, index) => {
            const isOpen = openQuestion === item.question
            const answerId = `faq-answer-${index}`
            const buttonId = `faq-question-${index}`
            return (
              <article className={`faq-item ${isOpen ? 'expanded' : ''}`} key={item.question}>
                <button
                  id={buttonId}
                  className="faq-question"
                  onClick={() => setOpenQuestion(isOpen ? '' : item.question)}
                  aria-expanded={isOpen}
                  aria-controls={answerId}
                >
                  {item.question}
                  <span className="toggle-icon" aria-hidden="true">{isOpen ? '-' : '+'}</span>
                </button>
                {isOpen && (
                  <div
                    id={answerId}
                    className="faq-answer"
                    role="region"
                    aria-labelledby={buttonId}
                  >
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
