import { useState, useEffect, useCallback } from 'react';
import { 
  Terminal, 
  Activity, 
  Settings, 
  Database, 
  Wrench, 
  Globe, 
  Award, 
  Moon, 
  Sun, 
  Github, 
  Cpu, 
  RefreshCw,
  AlertTriangle,
  CheckCircle2,
  XCircle
} from 'lucide-react';

interface SystemStatus {
  status: string;
  application: string;
  version: string;
  databaseConnection: string;
  totalStatusChecks?: number;
  databaseError?: string;
}

function App() {
  const [isDarkMode, setIsDarkMode] = useState<boolean>(() => {
    const saved = localStorage.getItem('theme');
    return saved ? saved === 'dark' : true; // Default to dark mode
  });
  
  const [status, setStatus] = useState<SystemStatus | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isRefreshing, setIsRefreshing] = useState<boolean>(false);
  const [activeTab, setActiveTab] = useState<'dashboard' | 'roadmap'>('dashboard');

  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5057';

  const checkStatus = useCallback(async () => {
    setIsRefreshing(true);
    setError(null);
    try {
      const response = await fetch(`${apiUrl}/api/v1/system/status`);
      if (!response.ok) {
        throw new Error(`Server returned status code: ${response.status}`);
      }
      const data = await response.json();
      setStatus(data);
    } catch (err: any) {
      console.error("API Fetch Error:", err);
      setError(err.message || 'Failed to connect to the backend API.');
      setStatus(null);
    } finally {
      setLoading(false);
      setIsRefreshing(false);
    }
  }, [apiUrl]);

  useEffect(() => {
    checkStatus();
    // Auto-refresh every 30 seconds
    const interval = setInterval(checkStatus, 30000);
    return () => clearInterval(interval);
  }, [checkStatus]);

  // Handle theme changes
  useEffect(() => {
    const root = window.document.documentElement;
    if (isDarkMode) {
      root.classList.add('dark');
      localStorage.setItem('theme', 'dark');
    } else {
      root.classList.remove('dark');
      localStorage.setItem('theme', 'light');
    }
  }, [isDarkMode]);

  const toggleTheme = () => setIsDarkMode(!isDarkMode);

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-800 dark:text-slate-100 font-sans transition-colors duration-300">
      {/* Header */}
      <header className="sticky top-0 z-40 w-full border-b border-slate-200 dark:border-slate-800 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <div className="bg-indigo-600 p-2 rounded-lg text-white">
              <Terminal className="h-6 w-6" />
            </div>
            <div>
              <span className="font-bold text-xl tracking-tight bg-gradient-to-r from-indigo-500 to-purple-500 bg-clip-text text-transparent">
                DevForge
              </span>
              <span className="ml-2 px-1.5 py-0.5 text-xs font-semibold bg-indigo-100 dark:bg-indigo-950 text-indigo-800 dark:text-indigo-300 rounded">
                v1.0
              </span>
            </div>
          </div>

          {/* Navigation Tabs */}
          <nav className="hidden md:flex space-x-1">
            <button
              onClick={() => setActiveTab('dashboard')}
              className={`px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                activeTab === 'dashboard'
                  ? 'bg-slate-100 dark:bg-slate-800 text-indigo-600 dark:text-indigo-400'
                  : 'text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800/50'
              }`}
            >
              Platform Overview
            </button>
            <button
              onClick={() => setActiveTab('roadmap')}
              className={`px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                activeTab === 'roadmap'
                  ? 'bg-slate-100 dark:bg-slate-800 text-indigo-600 dark:text-indigo-400'
                  : 'text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800/50'
              }`}
            >
              8-Week Roadmap
            </button>
          </nav>

          <div className="flex items-center space-x-4">
            {/* Live Connection Badge */}
            <div className="flex items-center space-x-2">
              {loading ? (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-100 dark:bg-amber-950 text-amber-800 dark:text-amber-300">
                  <RefreshCw className="animate-spin -ml-0.5 mr-1.5 h-3 w-3" />
                  Checking API...
                </span>
              ) : status ? (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-100 dark:bg-emerald-950 text-emerald-800 dark:text-emerald-300">
                  <span className="h-1.5 w-1.5 mr-1.5 bg-emerald-500 rounded-full animate-pulse"></span>
                  API: Online
                </span>
              ) : (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-rose-100 dark:bg-rose-950 text-rose-800 dark:text-rose-300">
                  <span className="h-1.5 w-1.5 mr-1.5 bg-rose-500 rounded-full"></span>
                  API: Offline
                </span>
              )}
            </div>

            {/* Dark Mode Toggle */}
            <button
              onClick={toggleTheme}
              className="p-2 rounded-lg border border-slate-200 dark:border-slate-800 text-slate-500 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
              aria-label="Toggle theme"
            >
              {isDarkMode ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </button>
          </div>
        </div>
      </header>

      {/* Main Content Container */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Mobile Navigation */}
        <div className="flex md:hidden justify-center mb-8 border-b border-slate-200 dark:border-slate-800 pb-3">
          <div className="flex space-x-2">
            <button
              onClick={() => setActiveTab('dashboard')}
              className={`px-3 py-1.5 rounded-md text-xs font-semibold ${
                activeTab === 'dashboard'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300'
              }`}
            >
              Dashboard
            </button>
            <button
              onClick={() => setActiveTab('roadmap')}
              className={`px-3 py-1.5 rounded-md text-xs font-semibold ${
                activeTab === 'roadmap'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300'
              }`}
            >
              Roadmap
            </button>
          </div>
        </div>

        {activeTab === 'dashboard' ? (
          <>
            {/* Hero Section */}
            <section className="text-center mb-16">
              <h1 className="text-4xl sm:text-5xl lg:text-6xl font-extrabold tracking-tight text-slate-900 dark:text-white mb-6">
                DevForge
              </h1>
              <p className="text-xl sm:text-2xl text-slate-600 dark:text-slate-400 max-w-3xl mx-auto leading-relaxed">
                Developer tools, API utilities, and .NET engineering challenges built for serious production platforms.
              </p>
              <div className="mt-8 flex justify-center space-x-4">
                <a 
                  href="#connectivity-panel"
                  className="px-6 py-3 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white font-medium shadow-md shadow-indigo-600/20 hover:shadow-lg transition-all"
                >
                  Verify Status
                </a>
                <button
                  onClick={() => setActiveTab('roadmap')}
                  className="px-6 py-3 rounded-lg bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 hover:border-slate-300 dark:hover:border-slate-700 text-slate-700 dark:text-slate-300 font-medium transition-all"
                >
                  View Roadmap
                </button>
              </div>
            </section>

            {/* Core Pillars Grid */}
            <section className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-16">
              {/* Pillar 1: Developer Tools */}
              <div className="relative group overflow-hidden bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 p-6 shadow-sm hover:shadow-md transition-all">
                <div className="flex justify-between items-start mb-6">
                  <div className="p-3 bg-blue-50 dark:bg-blue-950/50 text-blue-600 dark:text-blue-400 rounded-xl">
                    <Wrench className="h-6 w-6" />
                  </div>
                  <span className="px-2 py-1 text-xs font-semibold text-blue-800 dark:text-blue-300 bg-blue-100 dark:bg-blue-950 rounded-full">
                    Week 3
                  </span>
                </div>
                <h3 className="text-xl font-bold mb-2">Developer Tools</h3>
                <p className="text-slate-500 dark:text-slate-400 text-sm mb-4">
                  Essential developer utilities running entirely client-side for ultra-fast, zero-latency processing.
                </p>
                <ul className="space-y-2 text-xs text-slate-600 dark:text-slate-400 border-t border-slate-100 dark:border-slate-800 pt-4">
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-blue-500 rounded-full mr-2"></span> JSON Formatter & Validator
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-blue-500 rounded-full mr-2"></span> JWT Decoder & Debugger
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-blue-500 rounded-full mr-2"></span> Base64 Encoder / Decoder
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-blue-500 rounded-full mr-2"></span> SQL Formatter & GUID Generator
                  </li>
                </ul>
              </div>

              {/* Pillar 2: API Playground */}
              <div className="relative group overflow-hidden bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 p-6 shadow-sm hover:shadow-md transition-all">
                <div className="flex justify-between items-start mb-6">
                  <div className="p-3 bg-purple-50 dark:bg-purple-950/50 text-purple-600 dark:text-purple-400 rounded-xl">
                    <Globe className="h-6 w-6" />
                  </div>
                  <span className="px-2 py-1 text-xs font-semibold text-purple-800 dark:text-purple-300 bg-purple-100 dark:bg-purple-950 rounded-full">
                    Week 4
                  </span>
                </div>
                <h3 className="text-xl font-bold mb-2">API Playground</h3>
                <p className="text-slate-500 dark:text-slate-400 text-sm mb-4">
                  Interactive HTTP client to design, test, and save API collections right inside your browser dashboard.
                </p>
                <ul className="space-y-2 text-xs text-slate-600 dark:text-slate-400 border-t border-slate-100 dark:border-slate-800 pt-4">
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-purple-500 rounded-full mr-2"></span> Request Builder (GET/POST/PUT/...)
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-purple-500 rounded-full mr-2"></span> Custom Headers & Parameters
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-purple-500 rounded-full mr-2"></span> Response Payload Visualizer
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-purple-500 rounded-full mr-2"></span> Saved Queries & Collections
                  </li>
                </ul>
              </div>

              {/* Pillar 3: .NET Interview Challenge */}
              <div className="relative group overflow-hidden bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 p-6 shadow-sm hover:shadow-md transition-all">
                <div className="flex justify-between items-start mb-6">
                  <div className="p-3 bg-amber-50 dark:bg-amber-950/50 text-amber-600 dark:text-amber-400 rounded-xl">
                    <Award className="h-6 w-6" />
                  </div>
                  <span className="px-2 py-1 text-xs font-semibold text-amber-800 dark:text-amber-300 bg-amber-100 dark:bg-amber-950 rounded-full">
                    Week 5
                  </span>
                </div>
                <h3 className="text-xl font-bold mb-2">.NET Engineering</h3>
                <p className="text-slate-500 dark:text-slate-400 text-sm mb-4">
                  Advanced coding, architecture, and system design challenges built specifically for .NET architects.
                </p>
                <ul className="space-y-2 text-xs text-slate-600 dark:text-slate-400 border-t border-slate-100 dark:border-slate-800 pt-4">
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-amber-500 rounded-full mr-2"></span> C# and LINQ Practice Cases
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-amber-500 rounded-full mr-2"></span> ASP.NET Core & EF Core Tuning
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-amber-500 rounded-full mr-2"></span> SQL Server & Indexing Challenges
                  </li>
                  <li className="flex items-center">
                    <span className="h-1.5 w-1.5 bg-amber-500 rounded-full mr-2"></span> Microservices & System Design
                  </li>
                </ul>
              </div>
            </section>

            {/* Live API Connectivity Panel */}
            <section id="connectivity-panel" className="bg-slate-900 border border-slate-800 text-slate-200 rounded-2xl p-6 shadow-xl max-w-3xl mx-auto">
              <div className="flex items-center justify-between border-b border-slate-800 pb-4 mb-6">
                <div className="flex items-center space-x-2">
                  <div className="flex space-x-1.5">
                    <span className="h-3 w-3 bg-red-500 rounded-full"></span>
                    <span className="h-3 w-3 bg-yellow-500 rounded-full"></span>
                    <span className="h-3 w-3 bg-green-500 rounded-full"></span>
                  </div>
                  <span className="text-xs text-slate-500 font-mono ml-4">
                    terminal://devforge-status-client
                  </span>
                </div>
                <button
                  onClick={checkStatus}
                  disabled={isRefreshing}
                  className="p-1 rounded bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-slate-200 transition-colors disabled:opacity-50"
                  title="Manual refresh"
                >
                  <RefreshCw className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
                </button>
              </div>

              {/* Status Indicator Panel */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 font-mono text-sm">
                <div>
                  <h4 className="text-slate-500 font-bold mb-3 uppercase tracking-wider text-xs">
                    Client Connection Details
                  </h4>
                  <div className="space-y-2">
                    <div className="flex justify-between">
                      <span className="text-slate-400">Target Endpoint:</span>
                      <span className="text-blue-400 font-semibold">{apiUrl}/api/v1/system/status</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-400">Connection:</span>
                      {status ? (
                        <span className="text-emerald-400 flex items-center font-bold">
                          <CheckCircle2 className="h-4 w-4 mr-1 text-emerald-400" /> ACTIVE
                        </span>
                      ) : error ? (
                        <span className="text-rose-400 flex items-center font-bold">
                          <XCircle className="h-4 w-4 mr-1 text-rose-400" /> FAILED
                        </span>
                      ) : (
                        <span className="text-slate-400">CHECKING...</span>
                      )}
                    </div>
                    {error && (
                      <div className="mt-3 p-3 bg-rose-950/40 border border-rose-900/60 rounded text-rose-300 text-xs">
                        <AlertTriangle className="h-4 w-4 inline mr-1 text-rose-400" />
                        {error}
                      </div>
                    )}
                  </div>
                </div>

                <div>
                  <h4 className="text-slate-500 font-bold mb-3 uppercase tracking-wider text-xs">
                    Backend Service State
                  </h4>
                  {status ? (
                    <div className="space-y-2">
                      <div className="flex justify-between">
                        <span className="text-slate-400">App Name:</span>
                        <span className="text-indigo-400">{status.application}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-slate-400">App Version:</span>
                        <span>{status.version}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-slate-400">Database Engine:</span>
                        {status.databaseConnection === 'Connected' ? (
                          <span className="text-emerald-400 flex items-center">
                            <Database className="h-4 w-4 mr-1 text-emerald-400" /> Connected
                          </span>
                        ) : (
                          <span className="text-rose-400 flex items-center" title={status.databaseError}>
                            <AlertTriangle className="h-4 w-4 mr-1 text-rose-400 animate-pulse" /> Offline
                          </span>
                        )}
                      </div>
                      {status.totalStatusChecks !== undefined && (
                        <div className="flex justify-between">
                          <span className="text-slate-400">Total DB Writes:</span>
                          <span className="text-amber-400 font-semibold">{status.totalStatusChecks} checks</span>
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="flex flex-col items-center justify-center h-24 text-slate-600 bg-slate-950/40 rounded border border-slate-800/40">
                      <Activity className="h-8 w-8 mb-2 animate-pulse" />
                      <span className="text-xs">No active backend response</span>
                    </div>
                  )}
                </div>
              </div>
            </section>
          </>
        ) : (
          /* Roadmap Tab */
          <section className="max-w-4xl mx-auto bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 p-8 shadow-sm">
            <h2 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white mb-2">
              Implementation Roadmap
            </h2>
            <p className="text-slate-600 dark:text-slate-400 mb-8">
              DevForge is being developed over a rigorous 8-week production release cycle.
            </p>

            <div className="space-y-6">
              {/* Week 1 */}
              <div className="flex items-start">
                <div className="flex items-center justify-center h-8 w-8 rounded-full bg-emerald-100 dark:bg-emerald-950 text-emerald-800 dark:text-emerald-300 font-bold text-sm shrink-0">
                  W1
                </div>
                <div className="ml-4">
                  <h4 className="text-lg font-bold text-slate-900 dark:text-white flex items-center">
                    Foundation Setup
                    <span className="ml-2 px-2 py-0.5 text-xs font-semibold text-emerald-800 dark:text-emerald-300 bg-emerald-100 dark:bg-emerald-950 rounded">
                      Completed
                    </span>
                  </h4>
                  <p className="text-slate-600 dark:text-slate-400 text-sm mt-1">
                    Solution architecture, structured logging, SQL Server Integration via EF Core migrations, global exception filters, dynamic Swagger versioning, Dockerization, and CI actions.
                  </p>
                </div>
              </div>

              {/* Week 2 */}
              <div className="flex items-start">
                <div className="flex items-center justify-center h-8 w-8 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 font-bold text-sm shrink-0">
                  W2
                </div>
                <div className="ml-4">
                  <h4 className="text-lg font-bold text-slate-900 dark:text-white">Authentication & RBAC</h4>
                  <p className="text-slate-600 dark:text-slate-400 text-sm mt-1">
                    User registration, login, JWT authorization, Refresh Token rotation, and Role-Based Access Control filters.
                  </p>
                </div>
              </div>

              {/* Week 3 */}
              <div className="flex items-start">
                <div className="flex items-center justify-center h-8 w-8 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 font-bold text-sm shrink-0">
                  W3
                </div>
                <div className="ml-4">
                  <h4 className="text-lg font-bold text-slate-900 dark:text-white">Developer Tools Module</h4>
                  <p className="text-slate-600 dark:text-slate-400 text-sm mt-1">
                    Release of first major tool sets: JSON Formatters, SQL beautifiers, JWT decoder dashboard, Base64 processors, and HL7 validation.
                  </p>
                </div>
              </div>

              {/* Week 4 */}
              <div className="flex items-start">
                <div className="flex items-center justify-center h-8 w-8 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 font-bold text-sm shrink-0">
                  W4
                </div>
                <div className="ml-4">
                  <h4 className="text-lg font-bold text-slate-900 dark:text-white">API Playground</h4>
                  <p className="text-slate-600 dark:text-slate-400 text-sm mt-1">
                    Full-featured request composer supporting headers, parameter variables, JSON request bodies, and history storage.
                  </p>
                </div>
              </div>

              {/* Week 5 */}
              <div className="flex items-start">
                <div className="flex items-center justify-center h-8 w-8 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 font-bold text-sm shrink-0">
                  W5
                </div>
                <div className="ml-4">
                  <h4 className="text-lg font-bold text-slate-900 dark:text-white">.NET Interview Challenges</h4>
                  <p className="text-slate-600 dark:text-slate-400 text-sm mt-1">
                    C# Compiler simulation, LINQ puzzles, and architectural system design interactive tests for developer assessments.
                  </p>
                </div>
              </div>

              {/* Week 6-8 */}
              <div className="flex items-start">
                <div className="flex items-center justify-center h-8 w-8 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 font-bold text-sm shrink-0">
                  W6+
                </div>
                <div className="ml-4">
                  <h4 className="text-lg font-bold text-slate-900 dark:text-white">Administration, Optimization & Production Launch</h4>
                  <p className="text-slate-600 dark:text-slate-400 text-sm mt-1">
                    Admin moderation consoles, Redis caching layer, integration of end-to-end performance benchmarks, and deployment setup.
                  </p>
                </div>
              </div>
            </div>
          </section>
        )}
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 py-12 mt-12 transition-colors">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col md:flex-row items-center justify-between text-slate-500 dark:text-slate-400 text-sm">
          <div className="flex items-center space-x-2 mb-4 md:mb-0">
            <Cpu className="h-5 w-5 text-indigo-500" />
            <span className="font-semibold text-slate-700 dark:text-slate-300">DevForge Foundation Platform</span>
          </div>
          <div className="flex space-x-6 mb-4 md:mb-0">
            <button onClick={() => setActiveTab('dashboard')} className="hover:text-slate-700 dark:hover:text-slate-200">Dashboard</button>
            <button onClick={() => setActiveTab('roadmap')} className="hover:text-slate-700 dark:hover:text-slate-200">Roadmap</button>
            <a 
              href="https://github.com/example/devforge" 
              target="_blank" 
              rel="noopener noreferrer"
              className="hover:text-slate-700 dark:hover:text-slate-200 flex items-center"
            >
              <Github className="h-4 w-4 mr-1.5" /> GitHub
            </a>
          </div>
          <div>
            &copy; {new Date().getFullYear()} DevForge. Open source MIT license.
          </div>
        </div>
      </footer>
    </div>
  );
}

export default App;
