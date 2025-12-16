const AboutPage = () => {
  const stack = [
    {
      label: "Backend",
      color: "bg-blue-50 text-blue-600 border-blue-100",
      dot: "bg-blue-500",
      items: [
        ".NET 10",
        "Clean Architecture",
        "CQRS + MediatR",
        "Entity Framework Core",
        "PostgreSQL",
        "Hangfire",
      ],
    },
    {
      label: "Frontend",
      color: "bg-violet-50 text-violet-600 border-violet-100",
      dot: "bg-violet-500",
      items: [
        "React 18",
        "TypeScript",
        "Vite",
        "Tailwind CSS",
        "TanStack Query",
        "React Hook Form + Zod",
      ],
    },
    {
      label: "AI & Processing",
      color: "bg-emerald-50 text-emerald-600 border-emerald-100",
      dot: "bg-emerald-500",
      items: [
        "Groq API",
        "PdfPig (PDF extraction)",
        "Background job pipeline",
        "Multi-step status tracking",
        "Webhook notifications",
      ],
    },
    {
      label: "Auth & Testing",
      color: "bg-amber-50 text-amber-600 border-amber-100",
      dot: "bg-amber-500",
      items: [
        "JWT authentication",
        "Multi-tenancy (EF Core filters)",
        "xUnit + integration tests",
        "WebApplicationFactory",
        "MailKit (SMTP)",
      ],
    },
  ];
  const challenges = [
    {
      title: "EF Core tracking & PostgreSQL concurrency",
      description:
        "AsNoTracking() combined with Attach + Modified caused extracted fields to silently not persist. Compounded by PostgreSQL's xmin row versioning bumping after ExtractionJob updates, causing EF Core to see stale versions on subsequent saves. Fixed by implementing dedicated repository methods using ExecuteUpdateAsync to bypass the change tracker entirely.",
    },
    {
      title: "Multi-tenant data isolation",
      description:
        "Tenant scoping is enforced at the EF Core level via global query filters on every entity rather than at the application layer, making cross-tenant data leakage structurally impossible regardless of how queries are written.",
    },
    {
      title: "Background processing pipeline",
      description:
        "Documents move through Uploaded → Queued → Processing → Completed via Hangfire jobs. Invalid status transitions and concurrency exceptions from multiple SaveChangesAsync calls on tracked entities required careful state machine design and precise sequencing.",
    },
    {
      title: "AI extraction with confidence scoring",
      description:
        "Groq API extracts structured fields from documents based on a configurable schema. PdfPig handles PDF text extraction before passing content to the model. Confidence scores are returned and persisted alongside each extracted field, with webhook notifications dispatched on completion or failure.",
    },
    {
      title: "Integration test isolation",
      description:
        "WebApplicationFactory-based integration tests use a unique in-memory database name per test instance (Guid.NewGuid()) to prevent state bleed between tests, a subtle but critical detail for reliable parallel test runs.",
    },
  ];
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">About</h1>
        <p className="text-gray-600 text-sm mt-1">
          What DocuFlow is and how it was built
        </p>
      </div>
      {/* Hero card */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center flex-shrink-0">
            <svg
              className="w-5 h-5 text-blue-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
          </div>
          <h2 className="text-sm font-semibold text-gray-900">
            What is DocuFlow?
          </h2>
        </div>
        <p className="text-sm text-gray-700 leading-relaxed">
          DocuFlow is a multi-tenant document processing SaaS built on .NET 10
          and React. Users upload invoices, PDFs, and spreadsheets, and the
          system automatically extracts structured data using AI. Every tenant's
          data is isolated at the database level. Documents are processed in the
          background via Hangfire, moving from upload through to extraction
          without any manual steps. The project is full stack end to end,
          covering Clean Architecture, CQRS, JWT auth, a React frontend, AI
          integration, webhook notifications, and integration tests.
        </p>
      </div>
      {/* Tech Stack */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-sm font-semibold text-gray-900 mb-4">Tech Stack</h2>
        <div className="grid grid-cols-2 gap-3">
          {stack.map((section) => (
            <div
              key={section.label}
              className="rounded-lg border border-gray-100 bg-slate-50 p-4"
            >
              <span
                className={`inline-block text-xs font-semibold px-2 py-0.5 rounded-md border mb-3 ${section.color}`}
              >
                {section.label}
              </span>
              <ul className="space-y-1.5">
                {section.items.map((item) => (
                  <li
                    key={item}
                    className="text-sm text-gray-700 flex items-center gap-2"
                  >
                    <div
                      className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${section.dot}`}
                    />
                    {item}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>
      {/* Engineering Challenges */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-sm font-semibold text-gray-900 mb-4">
          Key Engineering Challenges
        </h2>
        <div className="space-y-0 divide-y divide-gray-100">
          {challenges.map((c, i) => (
            <div key={c.title} className="py-4 first:pt-0 last:pb-0 flex gap-4">
              <div className="flex-shrink-0 w-6 h-6 rounded-full bg-blue-50 border border-blue-100 flex items-center justify-center mt-0.5">
                <span className="text-xs font-semibold text-blue-500">
                  {i + 1}
                </span>
              </div>
              <div>
                <p className="text-sm font-semibold text-gray-900">{c.title}</p>
                <p className="text-sm text-gray-600 mt-1 leading-relaxed">
                  {c.description}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>
      {/* Built by */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-sm font-semibold text-gray-900 mb-4">Built by</h2>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center flex-shrink-0">
              <span className="text-sm font-bold text-white">AC</span>
            </div>
            <div>
              <p className="text-sm font-semibold text-gray-900">
                Arvind Chauhan
              </p>
              <p className="text-xs text-gray-500 mt-0.5">
                Software Developer · NZ
              </p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <a
              href="https://github.com/erwin09chauhan/docuflow"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-2 text-sm font-medium text-gray-700 hover:text-gray-900 bg-slate-50 border border-gray-200 hover:border-gray-300 px-3 py-1.5 rounded-lg transition-colors"
            >
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" />
              </svg>
              GitHub
            </a>
          </div>
        </div>
      </div>
    </div>
  );
};
export default AboutPage;
