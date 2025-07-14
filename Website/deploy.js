import { execSync } from 'child_process';
import path from 'path';

try {
  console.log('Building the app...');
  execSync('npm run build', { stdio: 'inherit' });
  
  console.log('Deploying to Netlify...');
  execSync('netlify deploy --prod --dir=dist', { stdio: 'inherit' });
  
  console.log('Deployment complete!');
} catch (error) {
  console.error('Deployment failed:', error.message);
  process.exit(1);
}
