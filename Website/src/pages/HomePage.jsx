import { useState } from 'react';
import { Link } from 'react-router-dom';
import './HomePage.css';
const examples = [{
  label: 'A slow PC',
  question: 'Why is my PC slow?',
  answer: 'Let’s check what’s using your computer’s memory. Closing apps you aren’t using can give it some breathing room.',
  action: 'See what’s using memory',
  hint: 'Understand what’s happening before making changes.'
}, {
  label: 'Peace of mind',
  question: 'Is my computer protected?',
  answer: 'I can check Windows Security and show the status of your protection and most recent scan.',
  action: 'Check Windows protection',
  hint: 'A simpler way to check the protection already on your PC.'
}, {
  label: 'Everyday shortcuts',
  question: 'Open calculator',
  answer: 'Opening Calculator. You can also ask for other familiar Windows apps by name.',
  action: 'Open an app',
  hint: 'Less searching through menus. More getting on with your day.'
}];
const benefits = [['01', 'Make sense of a slow PC.', 'Find out which apps are using memory and what might be holding your computer back.'], ['02', 'Keep an eye on the essentials.', 'Check battery, connection details, and Windows protection in one place.'], ['03', 'Get to the things you need.', 'Ask to open an app instead of hunting through Windows menus.']];
export default function HomePage() {
  const [selected, setSelected] = useState(0);
  const example = examples[selected];
  return <div className="home-page">
    <section className="hero-showcase">
      <div className="container hero-grid">
        <div className="hero-copy">
          <p className="eyebrow"><span className="tiny-dot" /> A helping hand for your Windows PC</p>
          <h1>Your computer.<br /><span>A little less complicated.</span></h1>
          <p className="hero-lede">A slow laptop? Too many menus? Smartitecture helps you understand your PC and get everyday things done. Just ask in your own words.</p>
          <div className="hero-actions"><Link className="button large" to="/download">Try the Windows beta <span aria-hidden="true">&#8594;</span></Link><a className="text-link" href="#see-it">See how it helps <span aria-hidden="true">&#8594;</span></a></div>
          <p className="hero-note">For Windows 10 &amp; 11 &middot; Early-access software</p>
        </div>
        <div className="assistant-preview" id="see-it">
          <div className="preview-bar"><span className="preview-brand">S<span className="brand-spark">&#10022;</span></span><span>Meet your PC’s helping hand</span><span className="preview-dots" aria-hidden="true">...</span></div>
          <div className="example-picker" role="group" aria-label="Choose an example">
            {examples.map((item, index) => <button key={item.label} aria-pressed={selected === index} onClick={() => setSelected(index)}>{item.label}</button>)}
          </div>
          <div className="preview-thread" aria-live="polite" aria-atomic="true">
            <p className="preview-you">{example.question}</p>
            <div className="preview-reply"><span className="assistant-avatar" aria-hidden="true">&#10022;</span><div><strong>Smartitecture</strong><p>{example.answer}</p><span className="example-action">{example.action} <span aria-hidden="true">&#8594;</span></span></div></div>
          </div>
          <div className="preview-caption">Illustrative conversation &middot; No checks run on this website</div>
        </div>
      </div>
    </section>
    <div className="benefit-strip"><div className="container"><span>Made for everyday Windows users</span><span>Useful PC checks, even offline</span><span>You choose what happens next</span></div></div>
    <section className="section"><div className="container">
      <div className="section-intro"><p className="eyebrow">A little help goes a long way</p><h2>Less figuring it out.<br />More getting things done.</h2></div>
      <div className="capability-grid">{benefits.map(([number, title, text]) => <article className="capability-card" key={number}><span className="feature-number">{number}</span><h3>{title}</h3><p>{text}</p></article>)}</div>
    </div></section>
    <section className="section calm-section"><div className="container reassurance-grid"><div><p className="eyebrow">Your PC. Your say.</p><h2>Help that keeps<br />you in the loop.</h2></div><div><p>Start with a check. See what it finds. Decide what to do next. Smartitecture brings useful Windows information together so you can make sense of it.</p><p>PC checks happen on your computer. Broader AI conversations need an online connection and access to the AI service.</p><Link className="text-link" to="/faq">A few things worth knowing <span aria-hidden="true">&#8594;</span></Link></div></div></section>
    <section className="section"><div className="container closing-cta"><p className="eyebrow">Let’s make everyday computing easier</p><h2>Meet Smartitecture.</h2><p>Try the Windows beta and help shape what comes next.</p><Link className="button large" to="/download">Explore the beta <span aria-hidden="true">&#8594;</span></Link><p className="small-note">Still in development. Installation may need a few extra steps.</p></div></section>
  </div>;
}
