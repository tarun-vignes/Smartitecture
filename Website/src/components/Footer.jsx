import { Link } from 'react-router-dom'
import './Footer.css'

function Footer() {
  const currentYear = new Date().getFullYear()

  return (
    <footer className="footer">
      <div className="container footer-container">
        <div className="footer-grid">
          <div className="footer-brand">
            <div className="footer-logo">
              <span className="footer-logo-text">Smartitecture</span>
            </div>
            <p className="footer-tagline">
              Beta Windows assistant for AI answers, PC diagnostics, app launching, and safe automation.
            </p>
          </div>

          <div className="footer-links">
            <h3 className="footer-heading">Product</h3>
            <ul className="footer-nav">
              <li><Link to="/">Home</Link></li>
              <li><Link to="/features">Features</Link></li>
              <li><Link to="/download">Download beta</Link></li>
              <li><Link to="/faq">FAQ</Link></li>
            </ul>
          </div>

          <div className="footer-links">
            <h3 className="footer-heading">Project</h3>
            <ul className="footer-nav">
              <li>
                <a
                  href="https://github.com/tarun-vignes/Smartitecture"
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label="Smartitecture GitHub repository, opens in a new tab"
                >
                  GitHub
                </a>
              </li>
              <li>
                <a
                  href="https://smartitecture-backend.onrender.com/health"
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label="Smartitecture backend health endpoint, opens in a new tab"
                >
                  Backend health
                </a>
              </li>
              <li><Link to="/about">About</Link></li>
            </ul>
          </div>

          <div className="footer-status">
            <h3 className="footer-heading">Release Status</h3>
            <p>Version 1.0.0.1 beta is ready for controlled Windows testing.</p>
            <p>Mac and Linux support are future roadmap items, not current beta targets.</p>
          </div>
        </div>

        <div className="footer-bottom">
          <p className="copyright">&copy; {currentYear} Smartitecture.</p>
          <p>Beta software. Test before broad distribution.</p>
        </div>
      </div>
    </footer>
  )
}

export default Footer
