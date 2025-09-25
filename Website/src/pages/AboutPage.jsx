import { motion } from 'framer-motion'
import './AboutPage.css'

function AboutPage() {
  return (
    <div className="about-page">
      <section className="hero about-hero">
        <div className="container">
          <motion.div 
            className="hero-content"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <h1>About Smartitecture</h1>
            <p>Learn about our mission to make computer systems understandable for everyone</p>
          </motion.div>
        </div>
      </section>
      
      <section className="section about-mission">
        <div className="container">
          <motion.div 
            className="mission-content"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2>Our Mission</h2>
            <p>
              At Smartitecture, we believe that technology should be accessible to everyone, regardless of technical expertise. 
              Our mission is to bridge the digital divide by providing an intelligent assistant that helps users understand 
              and navigate their computer systems with confidence.
            </p>
            <p>
              We understand that technology can be complex and sometimes intimidating. 
              That's why we've created Smartitecture - to provide clear, accessible explanations and step-by-step guidance 
              that empowers users to solve problems independently.
            </p>
          </motion.div>
        </div>
      </section>
      
      <section className="section about-story">
        <div className="container">
          <motion.div 
            className="story-content"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2>Our Story</h2>
            <p>
              Smartitecture was born from a personal experience. Our founder watched as his grandmother struggled to use her new 
              computer, often feeling frustrated and calling family members for help with even simple tasks. He realized 
              there was a need for a solution that could provide immediate, patient assistance without making users feel 
              inadequate.
            </p>
            <p>
              After months of development and testing with elderly users, Smartitecture was created as a compassionate AI assistant 
              that specializes in explaining technology in simple terms, helping with common tasks, and protecting users 
              from online threats.
            </p>
          </motion.div>
        </div>
      </section>
      
      <section className="section about-team">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            Our Team
          </motion.h2>
          
          <motion.div 
            className="team-grid"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            <div className="team-member">
              <div className="member-photo"></div>
              <h3>John Smith</h3>
              <p className="member-role">Founder & CEO</p>
              <p className="member-bio">
                With a background in AI and a passion for making technology accessible, 
                John founded Smartitecture after seeing his grandmother struggle with her computer.
              </p>
            </div>
            
            <div className="team-member">
              <div className="member-photo"></div>
              <h3>Sarah Johnson</h3>
              <p className="member-role">Head of User Experience</p>
              <p className="member-bio">
                Sarah specializes in designing interfaces for elderly users and has conducted 
                extensive research on making technology more accessible.
              </p>
            </div>
            
            <div className="team-member">
              <div className="member-photo"></div>
              <h3>Michael Chen</h3>
              <p className="member-role">Lead AI Engineer</p>
              <p className="member-bio">
                Michael has over 10 years of experience in natural language processing and 
                conversational AI, making him perfect for developing Smartitecture's friendly communication style.
              </p>
            </div>
            
            <div className="team-member">
              <div className="member-photo"></div>
              <h3>Emma Rodriguez</h3>
              <p className="member-role">Security Specialist</p>
              <p className="member-bio">
                Emma ensures that Smartitecture provides the best security advice and protection 
                for elderly users who may be vulnerable to online scams.
              </p>
            </div>
          </motion.div>
        </div>
      </section>
      
      <section className="section about-values">
        <div className="container">
          <motion.h2 
            className="section-title"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            Our Values
          </motion.h2>
          
          <motion.div 
            className="values-grid"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            <div className="value-card">
              <h3>Accessibility</h3>
              <p>
                We believe technology should be accessible to everyone, regardless of age, 
                technical expertise, or ability.
              </p>
            </div>
            
            <div className="value-card">
              <h3>Empathy</h3>
              <p>
                We approach every interaction with understanding and patience, recognizing 
                that learning new technology can be challenging.
              </p>
            </div>
            
            <div className="value-card">
              <h3>Clarity</h3>
              <p>
                We communicate in clear, jargon-free language that anyone can understand, 
                making complex concepts simple.
              </p>
            </div>
            
            <div className="value-card">
              <h3>Security</h3>
              <p>
                We prioritize protecting our users from online threats and scams, providing 
                guidance that keeps them safe online.
              </p>
            </div>
          </motion.div>
        </div>
      </section>
    </div>
  )
}

export default AboutPage
