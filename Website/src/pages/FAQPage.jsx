import { useState } from 'react'
import { motion } from 'framer-motion'
import './FAQPage.css'

function FAQPage() {
  const [activeCategory, setActiveCategory] = useState('general')
  const [expandedQuestions, setExpandedQuestions] = useState({})
  
  const toggleQuestion = (id) => {
    setExpandedQuestions(prev => ({
      ...prev,
      [id]: !prev[id]
    }))
  }
  
  const faqCategories = [
    { id: 'general', name: 'General' },
    { id: 'installation', name: 'Installation' },
    { id: 'security', name: 'Security & Privacy' },
    { id: 'network', name: 'Wi-Fi & Network' },
    { id: 'accessibility', name: 'Accessibility' },
    { id: 'troubleshooting', name: 'Troubleshooting' }
  ]
  
  const faqQuestions = {
    general: [
      {
        id: 'general-1',
        question: 'What is AIPal?',
        answer: 'AIPal is a friendly AI companion designed to help elderly and less tech-savvy users navigate their computers with confidence. It provides simple, jargon-free explanations and step-by-step guidance for common computer tasks and problems.'
      },
      {
        id: 'general-2',
        question: 'Is AIPal free to use?',
        answer: 'Yes, AIPal is completely free for personal use. There are no hidden fees or subscriptions required.'
      },
      {
        id: 'general-3',
        question: 'What operating systems does AIPal support?',
        answer: 'Currently, AIPal is available for Windows 10 and Windows 11. We are working on versions for macOS and mobile devices in the future.'
      },
      {
        id: 'general-4',
        question: 'Does AIPal require an internet connection?',
        answer: 'AIPal requires an internet connection for most features, but basic functionality is available offline. For the best experience, we recommend having a stable internet connection.'
      },
      {
        id: 'general-5',
        question: 'How do I get started with AIPal?',
        answer: 'Simply download AIPal from our website, run the installer, and follow the on-screen instructions. Once installed, you can launch AIPal from your desktop or Start menu and start asking questions or requesting help.'
      }
    ],
    installation: [
      {
        id: 'installation-1',
        question: 'How do I install AIPal?',
        answer: 'Download the installer from our website, double-click the downloaded file, and follow the installation wizard. The process is simple and takes less than 5 minutes.'
      },
      {
        id: 'installation-2',
        question: 'Can I install AIPal on multiple computers?',
        answer: 'Yes, you can install AIPal on multiple computers for personal use. There are no restrictions on the number of installations.'
      },
      {
        id: 'installation-3',
        question: 'What are the system requirements for AIPal?',
        answer: 'AIPal requires Windows 10 or Windows 11, a 1.6 GHz or faster processor, 4 GB RAM, 500 MB of available storage space, and a broadband internet connection.'
      },
      {
        id: 'installation-4',
        question: 'How do I uninstall AIPal if needed?',
        answer: 'You can uninstall AIPal through the Windows Control Panel or Settings app like any other program. Go to "Add or Remove Programs," find AIPal in the list, and click "Uninstall."'
      },
      {
        id: 'installation-5',
        question: 'Will AIPal automatically update?',
        answer: 'Yes, AIPal will check for updates when you launch the application and prompt you when updates are available. You can also check for updates manually through the settings menu.'
      }
    ],
    security: [
      {
        id: 'security-1',
        question: 'Is my data secure with AIPal?',
        answer: 'Yes, AIPal prioritizes your privacy and security. All data is processed locally when possible, and any data sent to our servers is encrypted and never shared with third parties.'
      },
      {
        id: 'security-2',
        question: 'Can AIPal help me identify scams?',
        answer: 'Yes, AIPal can help identify potential scams, suspicious websites, and phishing attempts. It provides clear explanations about why something might be suspicious and how to protect yourself.'
      },
      {
        id: 'security-3',
        question: 'Does AIPal have access to my personal files?',
        answer: 'AIPal only accesses files and information that you explicitly share with it during your interaction. It does not scan or access your personal files without your permission.'
      },
      {
        id: 'security-4',
        question: 'How does AIPal protect my privacy?',
        answer: 'AIPal processes data locally whenever possible, minimizes data collection, and encrypts any data that needs to be processed on our servers. We do not sell or share your data with third parties.'
      },
      {
        id: 'security-5',
        question: 'Can AIPal help me create strong passwords?',
        answer: 'Yes, AIPal can provide guidance on creating strong, memorable passwords and can help you understand password managers and other security best practices.'
      }
    ],
    network: [
      {
        id: 'network-1',
        question: 'How can AIPal help with Wi-Fi issues?',
        answer: 'AIPal can help troubleshoot common Wi-Fi connection problems, explain network terminology in simple language, assess your Wi-Fi security, and provide step-by-step guidance for improving your connection.'
      },
      {
        id: 'network-2',
        question: 'Can AIPal check if my Wi-Fi is secure?',
        answer: 'Yes, AIPal can assess your current Wi-Fi connection security and provide recommendations for improvements if needed. It can explain different security types (WEP, WPA, WPA2, WPA3) in simple terms.'
      },
      {
        id: 'network-3',
        question: 'Will AIPal help me set up a new Wi-Fi network?',
        answer: 'Yes, AIPal can provide step-by-step guidance for setting up a new Wi-Fi network, including router placement, security settings, and connecting devices.'
      },
      {
        id: 'network-4',
        question: 'Can AIPal explain why my internet is slow?',
        answer: 'Yes, AIPal can help diagnose potential causes of slow internet, run speed tests, and provide recommendations for improving your connection speed.'
      },
      {
        id: 'network-5',
        question: 'Does AIPal help with other network issues besides Wi-Fi?',
        answer: 'Yes, AIPal can help with various network issues including wired connections, Bluetooth pairing problems, and general connectivity troubleshooting.'
      }
    ],
    accessibility: [
      {
        id: 'accessibility-1',
        question: 'What accessibility features does AIPal have?',
        answer: 'AIPal includes adjustable text size, high contrast mode, voice interaction, screen reader compatibility, and keyboard navigation support to accommodate users with different abilities.'
      },
      {
        id: 'accessibility-2',
        question: 'Can I interact with AIPal using my voice?',
        answer: 'Yes, AIPal supports voice interaction. You can speak your questions or commands, and AIPal will respond with both text and optional voice output.'
      },
      {
        id: 'accessibility-3',
        question: 'Does AIPal work with screen readers?',
        answer: 'Yes, AIPal is designed to be compatible with popular screen readers to ensure accessibility for users with visual impairments.'
      },
      {
        id: 'accessibility-4',
        question: 'Can I adjust the text size in AIPal?',
        answer: 'Yes, you can easily adjust the text size in AIPal through the settings menu to make it more comfortable to read.'
      },
      {
        id: 'accessibility-5',
        question: 'What languages does AIPal support?',
        answer: 'AIPal currently supports 30 languages, including English, Spanish, French, German, Italian, Chinese, Japanese, and many more. We are continuously adding support for additional languages.'
      }
    ],
    troubleshooting: [
      {
        id: 'troubleshooting-1',
        question: 'What should I do if AIPal is not responding?',
        answer: 'If AIPal is not responding, try closing and reopening the application. If the issue persists, restart your computer. If you still experience problems, check our support page for more troubleshooting steps.'
      },
      {
        id: 'troubleshooting-2',
        question: 'How do I report a problem with AIPal?',
        answer: 'You can report problems through the "Help & Feedback" section in the AIPal application or by contacting our support team through the Contact page on our website.'
      },
      {
        id: 'troubleshooting-3',
        question: 'AIPal didn\'t understand my question. What should I do?',
        answer: 'Try rephrasing your question using simpler language. If AIPal still doesn\'t understand, you can provide feedback through the application to help us improve.'
      },
      {
        id: 'troubleshooting-4',
        question: 'Can AIPal help with printer problems?',
        answer: 'Yes, AIPal can help troubleshoot common printer issues, explain error messages, and provide step-by-step guidance for setting up or fixing printer connections.'
      },
      {
        id: 'troubleshooting-5',
        question: 'What if AIPal gives me incorrect information?',
        answer: 'While we strive for accuracy, if you believe AIPal has provided incorrect information, please let us know through the feedback option in the application. This helps us improve AIPal for everyone.'
      }
    ]
  }
  
  return (
    <div className="faq-page">
      <section className="hero faq-hero">
        <div className="container">
          <motion.div 
            className="hero-content"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <h1>Frequently Asked Questions</h1>
            <p>Find answers to common questions about AIPal</p>
          </motion.div>
        </div>
      </section>
      
      <section className="section faq-main">
        <div className="container">
          <div className="faq-container">
            <motion.div 
              className="faq-categories"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              {faqCategories.map(category => (
                <button 
                  key={category.id}
                  className={`category-button ${activeCategory === category.id ? 'active' : ''}`}
                  onClick={() => setActiveCategory(category.id)}
                >
                  {category.name}
                </button>
              ))}
            </motion.div>
            
            <motion.div 
              className="faq-questions"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.4 }}
            >
              {faqQuestions[activeCategory].map(item => (
                <div 
                  key={item.id} 
                  className={`faq-item ${expandedQuestions[item.id] ? 'expanded' : ''}`}
                >
                  <button 
                    className="faq-question"
                    onClick={() => toggleQuestion(item.id)}
                    aria-expanded={expandedQuestions[item.id]}
                    aria-controls={`answer-${item.id}`}
                  >
                    {item.question}
                    <span className="toggle-icon">
                      {expandedQuestions[item.id] ? '−' : '+'}
                    </span>
                  </button>
                  <div 
                    id={`answer-${item.id}`}
                    className="faq-answer"
                    style={{ 
                      maxHeight: expandedQuestions[item.id] ? '1000px' : '0',
                      opacity: expandedQuestions[item.id] ? 1 : 0
                    }}
                  >
                    <p>{item.answer}</p>
                  </div>
                </div>
              ))}
            </motion.div>
          </div>
        </div>
      </section>
      
      <section className="section still-have-questions">
        <div className="container">
          <motion.div 
            className="questions-content"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2>Still Have Questions?</h2>
            <p>
              If you couldn't find the answer you were looking for, our support team is here to help.
              Contact us and we'll get back to you as soon as possible.
            </p>
            <a href="/contact" className="button">Contact Support</a>
          </motion.div>
        </div>
      </section>
    </div>
  )
}

export default FAQPage
