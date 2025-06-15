import { motion } from 'framer-motion'
import './DownloadPage.css'

function DownloadPage() {
  return (
    <div className="download-page">
      <section className="hero download-hero">
        <div className="container">
          <motion.div 
            className="hero-content"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <h1>Download Smartitecture</h1>
            <p>Get your intelligent system assistant today</p>
          </motion.div>
        </div>
      </section>
      
      <section className="section download-main">
        <div className="container">
          <div className="download-grid">
            <motion.div 
              className="download-info"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              <h2>Smartitecture for Windows</h2>
              <p className="version">Version 1.0.0</p>
              <p className="release-date">Released: May 15, 2025</p>
              <p className="description">
                Smartitecture is designed to help users understand their computer systems with confidence. 
                With clear explanations, security protection, and intelligent assistance, Smartitecture makes technology 
                accessible to everyone.
              </p>
              
              <div className="system-requirements">
                <h3>System Requirements</h3>
                <ul>
                  <li><strong>Operating System:</strong> Windows 10 or Windows 11</li>
                  <li><strong>Processor:</strong> 1.6 GHz or faster</li>
                  <li><strong>Memory:</strong> 4 GB RAM</li>
                  <li><strong>Storage:</strong> 500 MB available space</li>
                  <li><strong>Internet:</strong> Broadband connection</li>
                </ul>
              </div>
            </motion.div>
            
            <motion.div 
              className="download-card"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.6, delay: 0.4 }}
            >
              <div className="download-logo">
                <div className="logo-icon"></div>
                <span className="logo-text">Smartitecture</span>
              </div>
              
              <div className="download-buttons">
                <a href="#" className="button large download-button">
                  <span className="download-icon">⬇️</span>
                  Download for Windows
                </a>
                <p className="file-info">Smartitecture_Setup.exe (45 MB)</p>
              </div>
              
              <div className="alternative-downloads">
                <h3>Alternative Downloads</h3>
                <ul>
                  <li>
                    <a href="https://www.microsoft.com/store/search?q=Smartitecture" className="alt-download-link" target="_blank" rel="noopener noreferrer">
                      Microsoft Store
                    </a>
                  </li>
                  <li>
                    <a href="#" className="alt-download-link">
                      Portable Version (ZIP)
                    </a>
                  </li>
                  <li>
                    <a href="#" className="alt-download-link">
                      Previous Versions
                    </a>
                  </li>
                </ul>
              </div>
            </motion.div>
          </div>
        </div>
      </section>
      
      <section className="section installation-guide">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            Installation Guide
          </motion.h2>
          
          <motion.div 
            className="steps-container"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            <div className="installation-step">
              <div className="step-number">1</div>
              <div className="step-content">
                <h3>Download the Installer</h3>
                <p>Click the "Download for Windows" button above to download the installation file.</p>
                <div className="step-image"></div>
              </div>
            </div>
            
            <div className="installation-step">
              <div className="step-number">2</div>
              <div className="step-content">
                <h3>Run the Installer</h3>
                <p>Locate the downloaded file (usually in your Downloads folder) and double-click it to start the installation.</p>
                <div className="step-image"></div>
              </div>
            </div>
            
            <div className="installation-step">
              <div className="step-number">3</div>
              <div className="step-content">
                <h3>Follow the Instructions</h3>
                <p>The installation wizard will guide you through the setup process. Click "Next" to proceed through each step.</p>
                <div className="step-image"></div>
              </div>
            </div>
            
            <div className="installation-step">
              <div className="step-number">4</div>
              <div className="step-content">
                <h3>Launch Smartitecture</h3>
                <p>Once installation is complete, you can launch Smartitecture from your desktop or Start menu.</p>
                <div className="step-image"></div>
              </div>
            </div>
          </motion.div>
          
          <div className="installation-help">
            <h3>Need Help?</h3>
            <p>If you're having trouble installing Smartitecture, you can:</p>
            <ul>
              <li>View our <a href="#">detailed installation guide</a></li>
              <li>Watch our <a href="#">installation video tutorial</a></li>
              <li>Contact our <a href="/contact">support team</a></li>
            </ul>
          </div>
        </div>
      </section>
      
      <section className="section faq-section">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            Frequently Asked Questions
          </motion.h2>
          
          <motion.div 
            className="faq-container"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            <div className="faq-item">
              <h3>Is Smartitecture free to use?</h3>
              <p>Yes, Smartitecture is completely free for personal use. There are no hidden fees or subscriptions required.</p>
            </div>
            
            <div className="faq-item">
              <h3>Does Smartitecture work offline?</h3>
              <p>Smartitecture requires an internet connection for most features, but basic functionality is available offline.</p>
            </div>
            
            <div className="faq-item">
              <h3>Is my data secure with Smartitecture?</h3>
              <p>Yes, Smartitecture prioritizes your privacy and security. All data is processed locally when possible, and any data sent to our servers is encrypted and never shared with third parties.</p>
            </div>
            
            <div className="faq-item">
              <h3>Can I uninstall Smartitecture if needed?</h3>
              <p>Yes, you can easily uninstall Smartitecture through the Windows Control Panel or Settings app like any other program.</p>
            </div>
          </motion.div>
          
          <div className="faq-more">
            <p>Have more questions? Visit our <a href="/faq">complete FAQ page</a>.</p>
          </div>
        </div>
      </section>
    </div>
  )
}

export default DownloadPage
