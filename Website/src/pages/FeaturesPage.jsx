import React from 'react';
import { motion } from 'framer-motion';
import HandIcon from '../components/HandIcon';
import './FeaturesPage.css'

function FeaturesPage() {
  return (
    <div className="features-page">
      <section className="hero features-hero">
        <div className="container">
          <motion.div 
            className="hero-content"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <h1>Smartitecture Features</h1>
            <p>Discover how Smartitecture can help you understand your computer system with confidence</p>
          </motion.div>
        </div>
      </section>
      
      <section className="section features-overview">
        <div className="container">
          <motion.div 
            className="overview-content"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2>Designed for Understanding</h2>
            <p>
              Smartitecture is designed with all users in mind. Our features focus on making 
              computer systems understandable through clear explanations, step-by-step guidance, and proactive protection.
            </p>
          </motion.div>
        </div>
      </section>
      
      <section className="section feature-details">
        <div className="container">
          <div className="feature-item">
            <motion.div 
              className="feature-content"
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <h2>Security & Privacy Protection</h2>
              <p>
                Smartitecture helps protect you from online scams, suspicious websites, and potential security threats. 
                With simple explanations and alerts, you'll understand what's happening and how to stay safe.
              </p>
              <ul className="feature-list">
                <li>Scam detection and warnings</li>
                <li>Password strength assessment</li>
                <li>Suspicious website alerts</li>
                <li>Privacy setting recommendations</li>
                <li>Malware detection and removal guidance</li>
              </ul>
            </motion.div>
            <motion.div 
              className="feature-image"
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <div className="feature-image-container">
                <HandIcon size={300} className="feature-hand-icon" />
              </div>
            </motion.div>
          </div>
          
          <div className="feature-item reverse">
            <motion.div 
              className="feature-content"
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <h2>Wi-Fi & Network Assistance</h2>
              <p>
                Having trouble with your internet connection? Smartitecture can help you understand and solve common 
                Wi-Fi problems with simple, jargon-free explanations and step-by-step troubleshooting.
              </p>
              <ul className="feature-list">
                <li>Wi-Fi security assessment</li>
                <li>Connection troubleshooting</li>
                <li>Network terminology explanations</li>
                <li>Router setup guidance</li>
                <li>Internet speed optimization tips</li>
              </ul>
            </motion.div>
            <motion.div 
              className="feature-image"
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <div className="feature-image-container">
                <HandIcon size={300} className="feature-hand-icon" />
              </div>
            </motion.div>
          </div>
          
          <div className="feature-item">
            <motion.div 
              className="feature-content"
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <h2>System Optimization</h2>
              <p>
                Is your computer running slowly? Smartitecture can help you understand why and provide simple solutions 
                to improve performance without technical jargon.
              </p>
              <ul className="feature-list">
                <li>Performance analysis and recommendations</li>
                <li>Disk cleanup assistance</li>
                <li>Startup program management</li>
                <li>Memory usage optimization</li>
                <li>Update management assistance</li>
              </ul>
            </motion.div>
            <motion.div 
              className="feature-image"
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <div className="feature-image-container">
                <HandIcon size={300} className="feature-hand-icon" />
              </div>
            </motion.div>
          </div>
          
          <div className="feature-item reverse">
            <motion.div 
              className="feature-content"
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <h2>Screen Analysis & Guidance</h2>
              <p>
                Confused by what you're seeing on screen? Smartitecture can analyze what's on your screen and provide 
                clear explanations and guidance without you needing to take screenshots or describe the problem.
              </p>
              <ul className="feature-list">
                <li>Error message explanations</li>
                <li>Interface navigation guidance</li>
                <li>Button and menu explanations</li>
                <li>Step-by-step task guidance</li>
                <li>Visual element identification</li>
              </ul>
            </motion.div>
            <motion.div 
              className="feature-image"
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <div className="feature-image-container">
                <HandIcon size={300} className="feature-hand-icon" />
              </div>
            </motion.div>
          </div>
          
          <div className="feature-item">
            <motion.div 
              className="feature-content"
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <h2>Accessibility Features</h2>
              <p>
                Smartitecture is designed to be accessible to users of all abilities, with features that make it easier 
                to interact with your computer regardless of visual, hearing, or motor limitations.
              </p>
              <ul className="feature-list">
                <li>Adjustable text size</li>
                <li>High contrast mode</li>
                <li>Voice interaction</li>
                <li>Screen reader compatibility</li>
                <li>Keyboard navigation support</li>
              </ul>
            </motion.div>
            <motion.div 
              className="feature-image"
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <div className="feature-image-container">
                <HandIcon size={300} className="feature-hand-icon" />
              </div>
            </motion.div>
          </div>
          
          <div className="feature-item reverse">
            <motion.div 
              className="feature-content"
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <h2>Multi-Language Support</h2>
              <p>
                Smartitecture speaks your language! With support for 30 languages, you can communicate with Smartitecture 
                in the language you're most comfortable with.
              </p>
              <ul className="feature-list">
                <li>30 languages supported</li>
                <li>Automatic language detection</li>
                <li>Regional dialect understanding</li>
                <li>Cultural context awareness</li>
                <li>Language switching on demand</li>
              </ul>
            </motion.div>
            <motion.div 
              className="feature-image"
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <div className="feature-image-container">
                <HandIcon size={300} className="feature-hand-icon" />
              </div>
            </motion.div>
          </div>
        </div>
      </section>
      
      <section className="section feature-comparison">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            Smartitecture vs. Other Assistants
          </motion.h2>
          
          <motion.div 
            className="comparison-table-container"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            <div className="comparison-table">
              <div className="comparison-header">
                <div className="comparison-cell header-cell">Features</div>
                <div className="comparison-cell header-cell">Smartitecture</div>
                <div className="comparison-cell header-cell">Generic Assistants</div>
              </div>
              
              <div className="comparison-row">
                <div className="comparison-cell">Elderly-Focused Design</div>
                <div className="comparison-cell">✓</div>
                <div className="comparison-cell">✗</div>
              </div>
              
              <div className="comparison-row">
                <div className="comparison-cell">Jargon-Free Explanations</div>
                <div className="comparison-cell">✓</div>
                <div className="comparison-cell">Limited</div>
              </div>
              
              <div className="comparison-row">
                <div className="comparison-cell">Screen Analysis</div>
                <div className="comparison-cell">✓</div>
                <div className="comparison-cell">✗</div>
              </div>
              
              <div className="comparison-row">
                <div className="comparison-cell">Wi-Fi Security Assessment</div>
                <div className="comparison-cell">✓</div>
                <div className="comparison-cell">✗</div>
              </div>
              
              <div className="comparison-row">
                <div className="comparison-cell">Scam Detection</div>
                <div className="comparison-cell">✓</div>
                <div className="comparison-cell">Limited</div>
              </div>
              
              <div className="comparison-row">
                <div className="comparison-cell">Step-by-Step Guidance</div>
                <div className="comparison-cell">✓</div>
                <div className="comparison-cell">Limited</div>
              </div>
              
              <div className="comparison-row">
                <div className="comparison-cell">Privacy-Focused</div>
                <div className="comparison-cell">✓</div>
                <div className="comparison-cell">Varies</div>
              </div>
            </div>
          </motion.div>
        </div>
      </section>
      
      <section className="section cta-section">
        <div className="container">
          <motion.div 
            className="cta-content"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2>Ready to Experience Smartitecture?</h2>
            <p>Download today and start navigating technology with confidence.</p>
            <a href="/download" className="button large">Download Now</a>
          </motion.div>
        </div>
      </section>
    </div>
  )
}

export default FeaturesPage
