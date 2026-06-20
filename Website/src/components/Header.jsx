import { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import './Header.css'

function Header() {
  const [menuOpen, setMenuOpen] = useState(false)
  
  const toggleMenu = () => {
    setMenuOpen(!menuOpen)
  }
  
  return (
    <header className="header">
      <div className="container header-container">
        <div className="logo-container">
          <Link to="/" className="logo-link" aria-label="Smartitecture home">
            <div className="logo">
              <img src="/brand/smartitecture-logo.png" alt="" className="logo-image" aria-hidden="true" />
            </div>
          </Link>
        </div>
        
        <button 
          className={`mobile-menu-button ${menuOpen ? 'active' : ''}`} 
          onClick={toggleMenu}
          aria-label={menuOpen ? 'Close navigation menu' : 'Open navigation menu'}
          aria-controls="primary-navigation"
          aria-expanded={menuOpen}
        >
          <span className="menu-bar" aria-hidden="true"></span>
          <span className="menu-bar" aria-hidden="true"></span>
          <span className="menu-bar" aria-hidden="true"></span>
        </button>
        
        <nav
          id="primary-navigation"
          className={`main-nav ${menuOpen ? 'open' : ''}`}
          aria-label="Primary navigation"
        >
          <ul className="nav-list">
            <li className="nav-item">
              <NavLink to="/" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'} onClick={() => setMenuOpen(false)}>
                Home
              </NavLink>
            </li>
            <li className="nav-item">
              <NavLink to="/features" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'} onClick={() => setMenuOpen(false)}>
                Features
              </NavLink>
            </li>
            <li className="nav-item">
              <NavLink to="/download" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'} onClick={() => setMenuOpen(false)}>
                Download
              </NavLink>
            </li>
            <li className="nav-item">
              <NavLink to="/faq" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'} onClick={() => setMenuOpen(false)}>
                FAQ
              </NavLink>
            </li>
            <li className="nav-item">
              <NavLink to="/about" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'} onClick={() => setMenuOpen(false)}>
                About
              </NavLink>
            </li>
          </ul>
        </nav>
      </div>
    </header>
  )
}

export default Header
