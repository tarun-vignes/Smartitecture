import { motion } from 'framer-motion'
import './DownloadPage.css'

const packageDetails = [
  ['Version', '1.0.0.1 beta'],
  ['Platform', 'Windows 10 / Windows 11'],
  ['Backend', 'https://smartitecture-backend.onrender.com'],
  ['Package SHA256', 'F8D6F60785F5A5B10ED4FE63E9821A808E74CF0ACB79143311BAAFA353D3E2ED'],
  ['Portable ZIP SHA256', 'F632F4D6D3D661F35859D24029D0E149B331CB2169A57E8E7EA47C98E8274374']
]

const releaseBaseUrl = 'https://github.com/tarun-vignes/Smartitecture/releases/download/v1.0.0.1-beta'

function DownloadPage() {
  return (
    <div className="download-page">
      <section className="download-hero">
        <div className="container">
          <motion.div
            className="download-hero-copy"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <p className="eyebrow">Beta release</p>
            <h1>Download Smartitecture for Windows</h1>
            <p>
              Use the signed MSIX package for normal beta testing, or the portable ZIP when you need
              a no-installer build.
            </p>
          </motion.div>
        </div>
      </section>

      <section className="section download-main">
        <div className="container download-layout">
          <motion.article
            className="download-card primary-download"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45, delay: 0.1 }}
          >
            <h2>Recommended beta package</h2>
            <p>
              The current release artifact is generated locally and signed for beta validation. Upload
              this MSIX to your release host before making this page public.
            </p>
            <a
              className="button large download-button"
              href={`${releaseBaseUrl}/Smartitecture-1.0.0.1-win-x64.msix`}
            >
              Smartitecture-1.0.0.1-win-x64.msix
            </a>
            <p className="download-note">Signed Windows beta installer.</p>
          </motion.article>

          <motion.article
            className="download-card"
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45, delay: 0.2 }}
          >
            <h2>Portable build</h2>
            <p>
              The ZIP build is useful for internal testing and quick checks. It should be distributed
              only with clear beta instructions.
            </p>
            <a
              className="button secondary large download-button"
              href={`${releaseBaseUrl}/Smartitecture-win-x64.zip`}
            >
              Smartitecture-win-x64.zip
            </a>
            <p className="download-note">Portable build for internal testing.</p>
          </motion.article>
        </div>
      </section>

      <section className="section requirements-section">
        <div className="container requirements-grid">
          <div>
            <h2>System requirements</h2>
            <ul className="clean-list">
              <li>Windows 10 or Windows 11</li>
              <li>64-bit processor</li>
              <li>4 GB RAM minimum, 8 GB recommended</li>
              <li>Internet connection for AI server answers</li>
              <li>Microphone permission for voice input</li>
            </ul>
          </div>
          <div>
            <h2>Current release details</h2>
            <dl className="release-details">
              {packageDetails.map(([key, value]) => (
                <div key={key}>
                  <dt>{key}</dt>
                  <dd>{value}</dd>
                </div>
              ))}
            </dl>
          </div>
        </div>
      </section>

      <section className="section install-section">
        <div className="container">
          <h2>Beta install checklist</h2>
          <div className="install-grid">
            <article>
              <span>1</span>
              <h3>Install the package</h3>
              <p>Use the signed MSIX on a Windows test machine and confirm Smartitecture appears in Start.</p>
            </article>
            <article>
              <span>2</span>
              <h3>Connect the backend</h3>
              <p>Open Settings, set the Smartitecture backend URL, and add the beta API key.</p>
            </article>
            <article>
              <span>3</span>
              <h3>Run the smoke tests</h3>
              <p>Ask general questions, check performance, view scan results, and launch a few apps.</p>
            </article>
          </div>
        </div>
      </section>
    </div>
  )
}

export default DownloadPage
