import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import './HomePage.css'

function HomePage() {
  // Animation variants
  const fadeIn = {
    hidden: { opacity: 0, y: 20 },
    visible: { 
      opacity: 1, 
      y: 0,
      transition: { duration: 0.6 }
    }
  }
  
  const staggerContainer = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.2
      }
    }
  }
  
  return (
    <div className="home-page">
      {/* Hero Section */}
      <section className="hero">
        <div className="container">
          <motion.div 
            className="hero-content"
            initial="hidden"
            animate="visible"
            variants={staggerContainer}
          >
            <motion.h1 variants={fadeIn}>
              Intelligent System Understanding
            </motion.h1>
            <motion.p variants={fadeIn}>
              Smartitecture helps users understand their computer systems, 
              avoid scams, and solve problems with clear, accessible explanations.
            </motion.p>
            <motion.div className="hero-buttons" variants={fadeIn}>
              <Link to="/download" className="button large">
                Download Now
              </Link>
              <Link to="/features" className="button secondary large">
                Learn More
              </Link>
            </motion.div>
            <motion.div className="hero-image" variants={fadeIn}>
              <img 
                src="/src/assets/images/hero-image.png" 
                alt="Smartitecture interface showing a friendly conversation with a user" 
              />
            </motion.div>
          </motion.div>
        </div>
      </section>
      
      {/* Features Overview Section */}
      <section className="section features-overview">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            Designed for Simplicity
          </motion.h2>
          
          <motion.div 
            className="features-grid"
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            variants={staggerContainer}
          >
            <motion.div className="feature-card" variants={fadeIn}>
              <div className="feature-icon">🔒</div>
              <h3>Security & Privacy</h3>
              <p>Protect yourself from scams with simple explanations and alerts about potential security risks.</p>
            </motion.div>
            
            <motion.div className="feature-card" variants={fadeIn}>
              <div className="feature-icon">🖥️</div>
              <p>Optimize your computer's performance with one-click solutions and easy-to-understand recommendations.</p>
            </motion.div>
            
            <motion.div className="feature-card" variants={fadeIn}>
              <div className="feature-icon">🔍</div>
              <h3>Screen Analysis</h3>
              <p>Get help with confusing screens or error messages without needing to take screenshots.</p>
            </motion.div>
            
            <motion.div className="feature-card" variants={fadeIn}>
              <div className="feature-icon">🌐</div>
              <h3>Wi-Fi Help</h3>
              <p>Troubleshoot internet connection issues with step-by-step guidance in simple language.</p>
            </motion.div>
            
            <motion.div className="feature-card" variants={fadeIn}>
              <div className="feature-icon">🔤</div>
              <h3>Accessibility</h3>
              <p>Adjustable text size, high contrast mode, and read-aloud functionality for easier use.</p>
            </motion.div>
            
            <motion.div className="feature-card" variants={fadeIn}>
              <div className="feature-icon">🌍</div>
              <h3>Multiple Languages</h3>
              <p>Available in 30 languages to help users communicate in their preferred language.</p>
            </motion.div>
          </motion.div>
          
          <motion.div 
            className="features-cta"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.4 }}
          >
            <Link to="/features" className="button">
              Explore All Features
            </Link>
          </motion.div>
        </div>
      </section>
      
      {/* How It Works Section */}
      <section className="section how-it-works">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            How Smartitecture Works
          </motion.h2>
          
          <motion.div 
            className="steps-container"
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            variants={staggerContainer}
          >
            <motion.div className="step" variants={fadeIn}>
              <div className="step-number">1</div>
              <div className="step-content">
                <h3>Ask a Question</h3>
                <p>Type any computer question in plain language or use your voice to ask.</p>
              </div>
            </motion.div>
            
            <motion.div className="step" variants={fadeIn}>
              <div className="step-number">2</div>
              <div className="step-content">
                <h3>Get Simple Answers</h3>
                <p>Receive clear, jargon-free explanations that are easy to understand.</p>
              </div>
            </motion.div>
            
            <motion.div className="step" variants={fadeIn}>
              <div className="step-number">3</div>
              <div className="step-content">
                <h3>Solve Problems</h3>
                <p>Follow step-by-step guidance to fix issues or optimize your computer.</p>
              </div>
            </motion.div>
          </motion.div>
          
          <motion.div 
            className="demo-video"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.6 }}
          >
            <div className="video-placeholder">
              <div className="play-button">▶</div>
              <p>Watch Demo Video</p>
            </div>
          </motion.div>
        </div>
      </section>
      
      {/* Testimonials Section */}
      <section className="section testimonials">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            What Our Users Say
          </motion.h2>
          
          <motion.div 
            className="testimonials-grid"
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            variants={staggerContainer}
          >
            <motion.div className="testimonial-card" variants={fadeIn}>
              <p className="testimonial-quote">
                "Smartitecture has been a lifesaver for me. I used to call my grandchildren whenever I had computer problems, but now I can solve most issues myself. The simple explanations make technology much less intimidating."
              </p>
              <div className="testimonial-author">
                <div className="testimonial-avatar"></div>
                <div>
                  <p className="author-name">Margaret, 78</p>
                  <p className="author-location">Retired Teacher</p>
                </div>
              </div>
            </motion.div>
            
            <motion.div className="testimonial-card" variants={fadeIn}>
              <p className="testimonial-quote">
                "I almost fell for an online scam, but Smartitecture warned me about the suspicious website. The security features have given me confidence to use my computer without worrying about making a mistake."
              </p>
              <div className="testimonial-author">
                <div className="testimonial-avatar"></div>
                <div>
                  <p className="author-name">Robert, 72</p>
                  <p className="author-location">Retired Accountant</p>
                </div>
              </div>
            </motion.div>
            
            <motion.div className="testimonial-card" variants={fadeIn}>
              <p className="testimonial-quote">
                "I bought Smartitecture for my mother who was struggling with her new laptop. Now she's sending emails, joining video calls, and even shopping online with confidence. The screen analysis feature is particularly helpful."
              </p>
              <div className="testimonial-author">
                <div className="testimonial-avatar"></div>
                <div>
                  <p className="author-name">Sarah, 45</p>
                  <p className="author-location">Daughter of Smartitecture User</p>
                </div>
              </div>
            </motion.div>
          </motion.div>
        </div>
      </section>
      
      {/* Download CTA Section */}
      <section className="section download-cta">
        <div className="container">
          <motion.div 
            className="download-section"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2>Ready to Get Started?</h2>
            <p>Download Smartitecture today and experience a more accessible computing experience.</p>
            <div className="download-buttons">
              <Link to="/download" className="button large">
                Download Now
              </Link>
              <Link to="/contact" className="button secondary large">
                Contact Support
              </Link>
            </div>
          </motion.div>
        </div>
      </section>
    </div>
  )
}

export default HomePage
