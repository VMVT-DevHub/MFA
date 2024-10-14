import { useState } from "react";
import logo from "./assets/vmvt_logo.svg";
import "./App.css";
import Login from "./components/Login.jsx";
import AccessPassDisplay from "./components/AccessPassDisplay.jsx";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [accessPass, setAccessPass] = useState(null);
  const [isError, setIsError] = useState(false);

  const currentYear = new Date().getFullYear();

  const handleSubmit = async (username, password) => {
    setIsError(false);

    if (password.length <= 4) {
      setIsError(true);
      return;
    }

    const response = await fetch("/tap/login", {
      method: "POST",
      cache: "no-cache",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName: username, userPass: password }),
    });
    if (response.status === 200) {
      const responseData = await response.json();
      setIsLoggedIn(true);
      setAccessPass(responseData.accessPass);
    } else {
      setIsError(true);
    }
  };

  return (
    <div className="App">
      <div className="App-header">
        <img src={logo} className="App-logo" alt="VMVT logo" />
        <h1 className="App-text">MFA vienkartinis slaptažodis</h1>
      </div>

      <div className="App-container">
        {isLoggedIn ? (
          <AccessPassDisplay accessPass={accessPass} />
        ) : (
          <Login onSubmit={handleSubmit} isError={isError} />
        )}
      </div>

      <div className="App-footer">
        <div className="App-link">
          <a href="https://mysignins.microsoft.com/security-info">
            Konfigūruoti MFA
          </a>

          <a href="https://vmvtlt.sharepoint.com/SitePages/MFA--multi-factor-authentication.aspx">
            Instrukcija
          </a>
        </div>

        <p className="App-footer-text">&copy; {currentYear} VMVT</p>
      </div>
    </div>
  );
}

export default App;
