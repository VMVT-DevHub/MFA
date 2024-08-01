import React, { useState } from 'react';
import logo from './vmvt_logo.svg';

import './App.css';
import { Login } from './components/Login.jsx'
import { AccessPassDisplay } from './components/AccessPassDisplay.jsx';

function App() {

  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [accessPass, setAccessPass] = useState(null);
  const [isError, setIsError] = useState(false);

  const currentYear = new Date().getFullYear();

  const handleSubmit = async (username, password) => {
    setIsError(false);
    const response = await fetch("/tap/login", {
        method: "POST", cache: "no-cache",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({userName: username, userPass: password})
      });
    if (response.status === 200) {
      const responseData = await response.json();
      setIsLoggedIn(true);
      setAccessPass(responseData.accessPass);
    }
    else {
      setIsError(true);
    }
  };

  return (
    <div className="App">

      <div>
        <img src={logo} className="App-logo" alt="logo" />
        <p className="App-text">
          MFA vienkartinis slaptažodis
        </p>
      </div>

      <div className="App-container">
      {isLoggedIn ? 
      <AccessPassDisplay accessPass={accessPass}/> : 
      <Login onSubmit={handleSubmit} isError={isError}/>}
      </div>

      <div>
        <p className="App-footer-text">&copy; {currentYear} VMVT</p>
      </div>

    </div>
  );
}

export default App;
