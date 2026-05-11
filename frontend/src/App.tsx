import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Customers from './pages/Customers';
import Subscriptions from './pages/Subscriptions';
import PaymentHistory from './pages/PaymentHistory';

function App() {
  return (
    <BrowserRouter>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/customers" element={<Customers />} />
          <Route path="/subscriptions" element={<Subscriptions />} />
          <Route path="/payments" element={<PaymentHistory />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  );
}

export default App;
