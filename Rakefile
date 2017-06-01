require 'bundler/setup'
require 'albacore'
require 'albacore/tasks/release'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

Configuration = ENV['CONFIGURATION'] || 'Release'

Albacore::Tasks::Versionizer.new :versioning

desc 'create assembly infos'
asmver_files :assembly_info do |a|
  a.files = FileList['**/*.fsproj'] # optional, will find all projects recursively by default

  a.attributes assembly_description: "FsAttoparsec is A port of Bryan O'Sullivan's attoparsec from Haskell to F#.",
               assembly_configuration: Configuration,
               assembly_copyright: "(c) 2016 by pocketberserker",
               assembly_version: ENV['LONG_VERSION'],
               assembly_file_version: ENV['LONG_VERSION'],
               assembly_informational_version: ENV['BUILD_VERSION']
end

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_compile do |b|
  b.prop 'Configuration', Configuration
  b.logging = 'detailed'
  b.sln     = 'FsAttoparsec.sln'
end

task :paket_bootstrap do
  system 'tools/paket.bootstrapper.exe', clr_command: true unless   File.exists? 'tools/paket.exe'
end

desc 'restore all nugets as per the packages.config files'
task :restore => :paket_bootstrap do
  system 'tools/paket.exe', 'restore', clr_command: true
end

desc 'Perform full build'
build :compile => [:versioning, :restore, :assembly_info] do |b|
  b.prop 'Configuration', Configuration
  b.sln = 'FsAttoparsec.sln'
end

directory 'build/pkg'

desc 'package nugets - finds all projects and package them'
nugets_pack :create_nugets => ['build/pkg', :versioning, :compile] do |p|
  p.configuration = Configuration
  p.files   = [ 'FsAttoparsec.Tests/FsAttoparsec.Tests.fsproj' ]
  p.out     = 'build/pkg'
  p.exe     = 'packages/NuGet.CommandLine/tools/NuGet.exe'
  p.with_metadata do |m|
    m.id          = 'Attoparsec'
    m.title       = 'Attoparsec'
    m.description = "FsAttoparsec is A port of Bryan O'Sullivan's attoparsec from Haskell to F#. This fork is maintained by @haf – just aiming to give timely releases of the software to the community."
    m.authors     = 'pocketberserker, Anton Kropp, Henrik Feldt'
    m.project_url = 'https://github.com/haf/FsAttoparsec/'
    m.tags        = 'parsing, combinators, attoparsec'
    m.version     = ENV['NUGET_VERSION']
  end
end

namespace :tests do
  task :unit do
    system "FsAttoparsec.Tests/bin/#{Configuration}/FsAttoparsec.Tests.exe", clr_command: true
  end
end

task :tests => :'tests:unit'

task :default => [ :compile, :tests, :create_nugets ]

task :ensure_nuget_key do
  raise 'missing env NUGET_KEY value' unless ENV['NUGET_KEY']
end

Albacore::Tasks::Release.new :release,
                             pkg_dir: 'build/pkg',
                             depend_on: [:create_nugets, :ensure_nuget_key],
                             nuget_exe: 'packages/NuGet.CommandLine/tools/NuGet.exe',
                             api_key: ENV['NUGET_KEY']
