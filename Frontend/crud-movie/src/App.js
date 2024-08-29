import logo from './logo.svg';
import './App.css';
import { Container, Navbar, Nav } from 'react-bootstrap';
import { BrowserRouter, Link, Routes, Route } from 'react-router-dom';

import Landing from './pages/landing';
import Actors from './pages/actors';
function App() {
  return (
    <Container>
      <BrowserRouter>
        <Navbar bg='dark' variant='dark'>
          <Navbar.Brand as={Link} to="/">Movie World</Navbar.Brand>
          <Nav className = "mr-auto">
            <Nav.Link as={Link} to="/">Movies</Nav.Link>
            <Nav.Link as={Link} to="/actors">Actors</Nav.Link>
          </Nav>
        </Navbar>
        <Routes>
          <Route exact path="/"Component={() => <Landing />} />
          <Route exact path="/actors" Component={() => <Actors />} />
        </Routes>
      </BrowserRouter>
    </Container>
  );
}

export default App;
